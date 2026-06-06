using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Asv.Avalonia.InfoMessage;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Material.Icons;
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
    public string EmptyTileBrush { get; set; } = $"{Brushes.LightGreen}";
    public MapModeType Mode { get; set; } = MapModeType.Mixed;

    public override string ToString()
    {
        return $"Queue size: {RequestQueueSize}, Parallel: {RequestParallelThreads} thread";
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
    private readonly ILogger<TileLoader> _logger;
    private readonly Counter<int> _meterReq;
    private readonly Counter<int> _meterQueue;
    private readonly Counter<int> _meterDownload;
    private readonly IShellHost _shellHost;
    private readonly TileLoaderConfig _config;
    private readonly Lock _syncCfg = new();
    private long _totalRequests;
    private long _totalQueuedRequests;
    private long _totalNetworkRequests;
    private long _totalDownloadedTiles;
    private long _totalDownloadedBytes;
    private long _totalFailedDownloads;

    public TileLoader(
        ILogger<TileLoader> logger,
        IConfiguration configProvider,
        IMeterFactory meterFactory,
        IShellHost shellHost,
        [FromKeyedServices(FastTileCacheContract)] ITileCache fastCache,
        [FromKeyedServices(SlowTileCacheContract)] ITileCache slowCache
    )
    {
        _logger = logger;
        _shellHost = shellHost;
        _fastCache = fastCache;
        _slowCache = slowCache;
        _localRequests = new ConcurrentHashSet<TileKey>();
        _emptyBitmap = new ConcurrentDictionary<int, Bitmap>();
        _config = configProvider.Get<TileLoaderConfig>();
        CurrentMapMode = new SynchronizedReactiveProperty<MapModeType>(_config.Mode);
        EmptyTileBrush = new SynchronizedReactiveProperty<IBrush>(
            Brush.Parse(_config.EmptyTileBrush)
        );
        _onLoaded = new Subject<TileKey>();
        _requestQueue = Channel.CreateBounded<TileKey>(
            new BoundedChannelOptions(_config.RequestQueueSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
            }
        );
        DisposeCancel.Register(() => _requestQueue.Writer.TryComplete());

        for (var i = 0; i < _config.RequestParallelThreads; i++)
        {
            Task.Run(ProcessQueue);
        }

        var meter = meterFactory.Create(GeoMapMixin.MetricName);
        _meterReq = meter.CreateCounter<int>("loader_get");
        _meterQueue = meter.CreateCounter<int>("loader_queue_requests");
        _meterDownload = meter.CreateCounter<int>("loader_download_requests");

        _sub1 = CurrentMapMode
            .Skip(1)
            .Synchronize()
            .Subscribe(mode =>
            {
                using (_syncCfg.EnterScope())
                {
                    _config.Mode = mode;
                    configProvider.Set(_config);
                }
            });

        _sub2 = EmptyTileBrush
            .Skip(1)
            .Synchronize()
            .Subscribe(brush =>
            {
                using (_syncCfg.EnterScope())
                {
                    _config.EmptyTileBrush = brush.ToString() ?? _config.EmptyTileBrush;
                    configProvider.Set(_config);
                }
            });
    }

    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
    public SynchronizedReactiveProperty<MapModeType> CurrentMapMode { get; }
    public Observable<TileKey> OnLoaded => _onLoaded;

    public void Render(DrawingContext context, double x, double y, TileKey key)
    {
        _meterReq.Add(1);
        Interlocked.Increment(ref _totalRequests);
        using var refBitmap = _fastCache[key];
        if (refBitmap != null)
        {
            refBitmap.Render(context, x, y);
            return;
        }
        if (!_localRequests.Contains(key))
        {
            if (_requestQueue.Writer.TryWrite(key))
            {
                Interlocked.Increment(ref _totalQueuedRequests);
            }
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

                using var refBitmap = _fastCache[key];
                if (refBitmap != null)
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

                _meterDownload.Add(1);
                Interlocked.Increment(ref _totalNetworkRequests);

                var downloadedTile = await key.Provider.DownloadAsync(key, DisposeCancel);
                if (downloadedTile is null)
                {
                    continue;
                }

                Interlocked.Increment(ref _totalDownloadedTiles);
                Interlocked.Add(ref _totalDownloadedBytes, downloadedTile.CompressedSize);
                downloadedTile.AddRef();
                if (CurrentMapMode.Value != MapModeType.Online)
                {
                    _slowCache[key] = downloadedTile;
                }

                _fastCache[key] = downloadedTile;

                _onLoaded.OnNext(key);
            }
            catch (DownloadTileException ex)
            {
                Interlocked.Increment(ref _totalFailedDownloads);
                _shellHost.Shell?.RiseShellInfoMessage(
                    new ShellMessage(
                        ex.LocalizedTitle,
                        ex.LocalizedMessage,
                        ShellErrorState.Warning,
                        icon: MaterialIconKind.MapMarkerAlert,
                        duration: TimeSpan.FromSeconds(10)
                    )
                );
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalFailedDownloads);
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex, "Failed to load tile {key}", key);
            }
            finally
            {
                _localRequests.Remove(key);
            }
        }
    }

    public TileLoaderStatistic GetStatistic()
    {
        return new TileLoaderStatistic(
            Interlocked.Read(ref _totalRequests),
            _requestQueue.Reader.CanCount ? _requestQueue.Reader.Count : 0,
            _localRequests.Count,
            _config.RequestQueueSize,
            _config.RequestParallelThreads,
            Interlocked.Read(ref _totalQueuedRequests),
            Interlocked.Read(ref _totalNetworkRequests),
            Interlocked.Read(ref _totalDownloadedTiles),
            Interlocked.Read(ref _totalDownloadedBytes),
            Interlocked.Read(ref _totalFailedDownloads),
            CurrentMapMode.Value
        );
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
