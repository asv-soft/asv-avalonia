using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Channels;
using Asv.Common;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;
using BoundedChannelOptions = System.Threading.Channels.BoundedChannelOptions;
using HttpClient = System.Net.Http.HttpClient;

namespace Asv.Avalonia.Map;

public interface ITileLoader : IDisposable
{
    Bitmap this[TileKey position] { get; }
    Observable<TileLoadedEventArgs> OnLoaded { get; }
}

public class TileLoadedEventArgs(TileKey position, Bitmap? tile)
{
    public TileKey Position { get; } = position;
    public Bitmap? Tile { get; } = tile;
}

public class CacheTileLoaderConfig
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

public class CacheTileLoader : DisposableOnceWithCancel, ITileLoader
{
    private readonly ITileCache? _fastCache;
    private readonly ITileCache? _slowCache;
    private readonly ILogger<CacheTileLoader> _logger;
    private readonly HttpClient _httpClient;
    private readonly Channel<TileKey> _requestChannel;
    private readonly Subject<TileLoadedEventArgs> _onLoaded;
    private readonly ConcurrentHashSet<TileKey> _localRequests;
    private readonly ConcurrentHashSet<string> _remoteRequests;
    private readonly object _sync = new();
    private volatile Bitmap? _emptyBitmap;
    private readonly ImmutableDictionary<string, ITileProvider> _providers;
    private readonly IBrush _emptyTileBrush;

    public CacheTileLoader(
        ILoggerFactory logger,
        CacheTileLoaderConfig config,
        IEnumerable<ITileProvider> providers,
        ITileCache? fastCache,
        ITileCache? slowCache
    )
    {
        _fastCache = fastCache;
        _slowCache = slowCache;
        _logger = logger.CreateLogger<CacheTileLoader>();
        _providers = providers.ToImmutableDictionary(x => x.Id);
        _onLoaded = new();
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(config.RequestTimeoutMs),
        };
        _localRequests = new();
        _remoteRequests = new ConcurrentHashSet<string>();
        _requestChannel = Channel.CreateBounded<TileKey>(
            new BoundedChannelOptions(config.RequestQueueSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
            }
        );

        for (var i = 0; i < config.RequestParallelThreads; i++)
        {
            Task.Run(ProcessQueue);
        }

        _emptyTileBrush = Brush.Parse(config.EmptyTileBrush);

        _logger.ZLogInformation($"Run tile loaded with {config} ");
    }

    private async Task ProcessQueue()
    {
        await foreach (var position in _requestChannel.Reader.ReadAllAsync(DisposeCancel))
        {
            try
            {
                if (_localRequests.Add(position) == false)
                {
                    // already in progress => skip
                    continue;
                }

                for (var index = 0; index < _caches.Length; index++)
                {
                    var cache = _caches[index];
                    var tile = cache[position];
                    if (tile != null)
                    {
                        _onLoaded.OnNext(new TileLoadedEventArgs(position, tile));
                        break;
                    }
                }

                // try to get from cache
                if (_cache.TryGetValue<Bitmap>(position, out var cachedTile))
                {
                    _onLoaded.OnNext(new TileLoadedEventArgs(position, cachedTile));
                    continue;
                }
                // try load from disk
                var tilePath = GetTileCachePath(position, _provider);
                if (File.Exists(tilePath))
                {
                    var tile = new Bitmap(tilePath);
                    _cache.Set(
                        position,
                        tile,
                        new MemoryCacheEntryOptions
                        {
                            Size = tile.PixelSize.Width * tile.PixelSize.Height * 4,
                        }.RegisterPostEvictionCallback(RemoveTileCache)
                    );
                    _onLoaded.OnNext(new TileLoadedEventArgs(position, tile));
                    continue;
                }
                // load from server
                var url = _provider.GetTileUrl(position);
                Bitmap bitmap;
                if (url == null)
                {
                    bitmap = GetEmptyBitmap();
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

                    if (File.Exists(tilePath) == false)
                    {
                        await File.WriteAllBytesAsync(tilePath, img, DisposeCancel)
                            .ConfigureAwait(false);
                    }
                    else { }

                    bitmap = new Bitmap(new MemoryStream(img));
                    _remoteRequests.Remove(url);
                }
                _cache.Set(
                    position,
                    bitmap,
                    new MemoryCacheEntryOptions
                    {
                        Size = bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4,
                    }
                );
                _onLoaded.OnNext(new TileLoadedEventArgs(position, bitmap));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger?.LogError(ex, $"Failed to load tile {position}");
            }
            finally
            {
                _localRequests.Remove(position);
            }
        }
    }

    private void RemoveTileCache(object key, object? value, EvictionReason reason, object? state)
    {
        if (value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public Bitmap GetEmptyBitmap()
    {
        if (_emptyBitmap != null)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            return _emptyBitmap;
        }

        lock (_sync)
        {
            if (_emptyBitmap != null)
            {
                return _emptyBitmap;
            }

            var btm = new RenderTargetBitmap(new PixelSize(_provider.TileSize, _provider.TileSize));
            using (var ctx = btm.CreateDrawingContext(true))
            {
                ctx.FillRectangle(
                    _emptyTileBrush,
                    new Rect(0, 0, _provider.TileSize, _provider.TileSize)
                );
            }

            return _emptyBitmap = btm;
        }
    }

    private string GetTileCachePath(TilePosition position, ITileProvider provider)
    {
        // Генерируем путь на основе провайдера, zoom, X и Y координат тайла
        var providerName = provider.GetType().Name;
        var tileFolder = Path.Combine(_cacheDirectory, providerName, position.Zoom.ToString());

        if (!Directory.Exists(tileFolder))
        {
            Directory.CreateDirectory(tileFolder);
        }

        return Path.Combine(tileFolder, $"{position.X}_{position.Y}.png");
    }

    public Bitmap this[TileKey position]
    {
        get
        {
            _fast

            if (_localRequests.Contains(position) == false)
            {
                _requestChannel.Writer.TryWrite(position);
            }
            else { }

            return GetEmptyBitmap();
        }
    }

    public Observable<TileLoadedEventArgs> OnLoaded => _onLoaded;
}
