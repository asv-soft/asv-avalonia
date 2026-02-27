using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DotNext.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapServiceConfig
{
    public int RequestQueueSize { get; set; } = 100;
    public int RequestParallelThreads { get; set; } = Environment.ProcessorCount;
    public int RequestTimeoutMs { get; set; } = 5000;
    public string EmptyTileBrush { get; set; } = $"{Brushes.LightGreen}";

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
    private WriteableBitmap? _bitmap;

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
        var config = configProvider.Get<MapServiceConfig>();
        EmptyTileBrush = new ReactiveProperty<IBrush>(Brush.Parse(config.EmptyTileBrush));
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
    }

    private async Task ProcessQueue()
    {
        await foreach (var key in _requestQueue.Reader.ReadAllAsync(DisposeCancel))
        {
            _meterQueue.Add(1);
            try
            {
                if (_localRequests.Add(key) == false)
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

                var url = key.Provider.GetTileUrl(key);
                if (string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }
                else
                {
                    if (_remoteRequests.Add(url) == false)
                    {
                        continue;
                    }

                    try
                    {
                        _meterHttp.Add(1);
                        using var response = await _httpClient
                            .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, DisposeCancel)
                            .ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();

                        var contentLength =
                            response.Content.Headers.ContentLength.GetValueOrDefault(0);
                        var minSize = (int)Math.Min(contentLength, 1 * 1024 * 1024); // лимит, например 4MB
                        await response.Content.LoadIntoBufferAsync();
                        await using var stream = await response.Content.ReadAsStreamAsync();
                        tile = new Tile(key, stream);
                    }
                    finally
                    {
                        _remoteRequests.Remove(url);
                    }
                }
                tile.AddRef();
                _slowCache[key] = tile;
                _fastCache[key] = tile;
                _onLoaded.OnNext(key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger?.LogError(ex, $"Failed to load tile {key}");
            }
            finally
            {
                _localRequests.Remove(key);
            }
        }
    }

    public void GetBitmap(TileKey key, Action<Bitmap> onLoaded)
    {
        _meterReq.Add(1);
        using var tile = _fastCache[key];
        if (tile != null)
        {
            _bitmap ??= new WriteableBitmap(tile.PixelSize, tile.Dpi);
            tile.Write(_bitmap);
            onLoaded(_bitmap);
            return;
        }

        // we have no tile in fast cache => request it to load and return empty tile
        if (_localRequests.Contains(key) == false)
        {
            _requestQueue.Writer.TryWrite(key);
        }
        var bitMap = _emptyBitmap.GetOrAdd(
            key.Provider.TileSize,
            CreateEmptyBitmap,
            EmptyTileBrush.Value
        );
        onLoaded(bitMap);
    }

    private static Bitmap CreateEmptyBitmap(int size, IBrush brush)
    {
        var btm = new RenderTargetBitmap(new PixelSize(size, size));
        using var ctx = btm.CreateDrawingContext(true);
        ctx.FillRectangle(brush, new Rect(0, 0, size, size));
        return btm;
    }

    public ReactiveProperty<IBrush> EmptyTileBrush { get; }

    public Observable<TileKey> OnLoaded => _onLoaded;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fastCache.Dispose();
            _slowCache.Dispose();
            _localRequests.Dispose();
            _onLoaded.Dispose();
            _httpClient.Dispose();
            _remoteRequests.Dispose();
            EmptyTileBrush.Dispose();

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
}
