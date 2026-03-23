using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public enum MapModeType
{
    Mixed,
    Online,
    Offline,
}

public class TileLoaderConfig
{
    public int RequestQueueSize { get; set; } = 100;
    public int RequestParallelThreads { get; set; } = Environment.ProcessorCount;
    public int RequestTimeoutMs { get; set; } = 5000;
    public string EmptyTileBrush { get; set; } = $"{Brushes.LightGreen}";
    public MapModeType Mode { get; set; } = MapModeType.Mixed;

    public override string ToString()
    {
        return $"Queue size: {RequestQueueSize}, Parallel: {RequestParallelThreads} thread, Timeout: {RequestTimeoutMs} ms";
    }
}

public class TileLoader : AsyncDisposableWithCancel, ITileLoader
{
    public const string FastTileCacheContract = "map.cache.fast";
    public const string SlowTileCacheContract = "map.cache.slow";
    private readonly ITileCache _fastCache;
    private readonly ITileCache _slowCache;
    private readonly ConcurrentDictionary<int, Bitmap> _emptyBitmap;
    private readonly ConcurrentHashSet<TileKey> _localRequests;
    private readonly Channel<TileKey> _requestQueue;
    private readonly Subject<TileKey> _onLoaded;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentHashSet<string> _remoteRequests;
    private readonly ILogger<TileLoader> _logger;
    private readonly Counter<int> _meterReq;
    private readonly Counter<int> _meterQueue;
    private readonly Counter<int> _meterHttp;
    private readonly Lock _syncCfg = new();

    public TileLoader(
        ILoggerFactory loggerFactory,
        IConfiguration configProvider,
        IMeterFactory meterFactory,
        [FromKeyedServices(FastTileCacheContract)] ITileCache fastCache,
        [FromKeyedServices(SlowTileCacheContract)] ITileCache slowCache
    )
    {
        _logger = loggerFactory.CreateLogger<TileLoader>();
        _fastCache = fastCache; // new MemoryTileCache(new MemoryTileCacheConfig(), loggerFactory, meterFactory);
        _slowCache = slowCache; // new FileSystemCache(new FileSystemCacheConfig(), loggerFactory, meterFactory);
        _localRequests = new ConcurrentHashSet<TileKey>();
        _emptyBitmap = new ConcurrentDictionary<int, Bitmap>();
        var config = configProvider.Get<TileLoaderConfig>();
        CurrentMapMode = new SynchronizedReactiveProperty<MapModeType>(config.Mode);
        EmptyTileBrush = new SynchronizedReactiveProperty<IBrush>(
            Brush.Parse(config.EmptyTileBrush)
        );
        _onLoaded = new();
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(config.RequestTimeoutMs),
        };
        _remoteRequests = new ConcurrentHashSet<string>();
        _requestQueue = Channel.CreateBounded<TileKey>(
            new BoundedChannelOptions(config.RequestQueueSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
            }
        );

        for (var i = 0; i < config.RequestParallelThreads; i++)
        {
            Task.Run(ProcessQueue);
        }

        var meter = meterFactory.Create(GeoMapMixin.MetricName);
        _meterReq = meter.CreateCounter<int>("loader_get");
        _meterQueue = meter.CreateCounter<int>("loader_queue_requests");
        _meterHttp = meter.CreateCounter<int>("loader_http_requests");

        _sub1 = CurrentMapMode
            .Skip(1)
            .Synchronize()
            .Subscribe(mode =>
            {
                using (_syncCfg.EnterScope())
                {
                    config.Mode = mode;
                    configProvider.Set(config);
                }
            });

        _sub2 = EmptyTileBrush
            .Skip(1)
            .Synchronize()
            .Subscribe(brush =>
            {
                using (_syncCfg.EnterScope())
                {
                    config.EmptyTileBrush = brush.ToString() ?? config.EmptyTileBrush;
                    configProvider.Set(config);
                }
            });
    }

    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
    public SynchronizedReactiveProperty<MapModeType> CurrentMapMode { get; }
    public Observable<TileKey> OnLoaded => _onLoaded;

    public void Render(DrawingContext context, double x, double y, TileKey key)
    {
        _meterReq.Add(1);
        using var refBitmap = _fastCache[key];
        if (refBitmap != null)
        {
            refBitmap.Render(context, x, y);
            return;
        }
        if (!_localRequests.Contains(key))
        {
            _requestQueue.Writer.TryWrite(key);
        }
        context.DrawRectangle(
            EmptyTileBrush.Value,
            null,
            new Rect(x, y, key.Provider.TileSize, key.Provider.TileSize)
        );
    }

    private async Task ProcessQueue()
    {
        await foreach (var key in _requestQueue.Reader.ReadAllAsync(DisposeCancel))
        {
            _meterQueue.Add(1);
            try
            {
                if (!_localRequests.Add(key))
                {
                    // already in progress => skip
                    continue;
                }

                if (_fastCache[key] != null)
                {
                    continue;
                }

                var tile = _slowCache[key];
                if (tile != null)
                {
                    _fastCache[key] = tile;
                    _onLoaded.OnNext(key);
                    continue;
                }

                if (CurrentMapMode.Value == MapModeType.Offline)
                {
                    continue;
                }

                tile = await DownloadTileAsync(key, DisposeCancel);

                if (tile is null)
                {
                    continue;
                }

                tile.AddRef();
                if (CurrentMapMode.Value != MapModeType.Online)
                {
                    _slowCache[key] = tile;
                }

                _fastCache[key] = tile;

                _onLoaded.OnNext(key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex, "Failed to load tile {key}", key);
            }
            finally
            {
                _localRequests.Remove(key);
            }
        }
    }

    private async Task<Tile?> DownloadTileAsync(TileKey key, CancellationToken ct = default)
    {
        var url = key.Provider.GetTileUrl(key);
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!_remoteRequests.Add(url))
        {
            return null;
        }

        try
        {
            _meterHttp.Add(1);
            using var response = await _httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, DisposeCancel)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var contentLength = response.Content.Headers.ContentLength ?? 0;
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            if (contentLength == 0)
            {
                await response.Content.LoadIntoBufferAsync(ct);
                contentLength = stream.Length;
            }

            return Tile.Create(key, stream, (int)contentLength);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            _logger.LogError(ex, "Failed to download tile from remote with key - {key}", key);
            return null;
        }
        finally
        {
            _remoteRequests.Remove(url);
        }
    }

    #region Disposable

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _fastCache.Dispose();
            _slowCache.Dispose();
            _localRequests.Dispose();
            _onLoaded.Dispose();
            _httpClient.Dispose();
            _remoteRequests.Dispose();
            EmptyTileBrush.Dispose();
            CurrentMapMode.Dispose();

            foreach (var value in _emptyBitmap.Values)
            {
                value.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await _fastCache.DisposeAsync();

        await _slowCache.DisposeAsync();
        await CastAndDispose(_sub1);
        await CastAndDispose(_sub2);
        await CastAndDispose(CurrentMapMode);
        await CastAndDispose(_localRequests);
        await CastAndDispose(_onLoaded);
        await CastAndDispose(_httpClient);
        await CastAndDispose(_remoteRequests);
        await CastAndDispose(EmptyTileBrush);

        foreach (var value in _emptyBitmap.Values)
        {
            await CastAndDispose(value);
        }

        await base.DisposeAsyncCore();

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else
            {
                resource.Dispose();
            }
        }
    }

    #endregion
}
