using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Channels;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Map;

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

public class MapService : AsyncDisposableWithCancel, IMapService
{
    private readonly MemoryTileCache _fastCache;
    private readonly FileSystemCache _slowCache;
    private readonly ConcurrentDictionary<int, Bitmap> _emptyBitmap;
    private readonly ConcurrentHashSet<TileKey> _localRequests;
    private readonly Channel<TileKey> _requestQueue;
    private readonly ImmutableDictionary<string, ITileProvider> _providers;
    private readonly Subject<TileLoadedEventArgs> _onLoaded;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentHashSet<string> _remoteRequests;
    private readonly ILogger<MapService> _logger;

    public MapService(
        ILoggerFactory loggerFactory,
        IConfiguration configProvider,
        IEnumerable<ITileProvider> providers
    )
    {
        _logger = loggerFactory.CreateLogger<MapService>();
        _fastCache = new MemoryTileCache(new MemoryTileCacheConfig(), loggerFactory);
        _slowCache = new FileSystemCache(new FileSystemCacheConfig(), loggerFactory);
        _localRequests = new ConcurrentHashSet<TileKey>();
        _emptyBitmap = new ConcurrentDictionary<int, Bitmap>();
        var config = configProvider.Get<MapServiceConfig>();
        EmptyTileBrush = new ReactiveProperty<IBrush>(Brush.Parse(config.EmptyTileBrush));
        _providers = providers.ToImmutableDictionary(x => x.Info.Id);
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
    }

    private async Task ProcessQueue()
    {
        await foreach (var key in _requestQueue.Reader.ReadAllAsync(DisposeCancel))
        {
            try
            {
                if (_localRequests.Add(key) == false)
                {
                    // already in progress => skip
                    continue;
                }
                var tile = _slowCache[key];
                if (tile != null)
                {
                    _fastCache[key] = tile;
                    _onLoaded.OnNext(new TileLoadedEventArgs(key, tile));
                    break;
                }

                if (_providers.TryGetValue(key.Provider, out var provider) == false)
                {
                    throw new Exception($"Provider {key.Provider} not found");
                }

                var url = provider.GetTileUrl(key);
                if (string.IsNullOrWhiteSpace(url))
                {
                    tile = _emptyBitmap.GetOrAdd(
                        provider.TileSize,
                        CreateEmptyBitmap,
                        EmptyTileBrush.Value
                    );
                }
                else
                {
                    if (_remoteRequests.Add(url) == false)
                    {
                        continue;
                    }
                    var img = await _httpClient
                        .GetByteArrayAsync(url, DisposeCancel)
                        .ConfigureAwait(false);
                    tile = new Bitmap(new MemoryStream(img));
                }
                _slowCache[key] = tile;
                _fastCache[key] = tile;
                _onLoaded.OnNext(new TileLoadedEventArgs(key, tile));
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

    public IEnumerable<ITileProvider> Providers => _providers.Values;

    public Bitmap this[TileKey key]
    {
        get
        {
            var bitmap = _fastCache[key];
            if (bitmap != null)
            {
                return bitmap;
            }
            if (_localRequests.Contains(key) == false)
            {
                _requestQueue.Writer.TryWrite(key);
            }
            if (_providers.TryGetValue(key.Provider, out var provider))
            {
                return _emptyBitmap.GetOrAdd(
                    provider.TileSize,
                    CreateEmptyBitmap,
                    EmptyTileBrush.Value
                );
            }
            throw new Exception("Provider not found");
        }
    }

    private static Bitmap CreateEmptyBitmap(int size, IBrush brush)
    {
        var btm = new RenderTargetBitmap(new PixelSize(size, size));
        using var ctx = btm.CreateDrawingContext(true);
        ctx.FillRectangle(brush, new Rect(0, 0, size, size));
        return btm;
    }

    public ReactiveProperty<IBrush> EmptyTileBrush { get; }

    public Observable<TileLoadedEventArgs> OnLoaded => _onLoaded;
}
