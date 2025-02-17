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

namespace Asv.Avalonia.Map;

public interface ITileLoader : IDisposable
{
    Bitmap this[TilePosition position] { get; }
    Observable<TileLoadedEventArgs> OnLoaded { get; }
}

public class TileLoadedEventArgs(TilePosition position, Bitmap? tile)
{
    public TilePosition Position { get; } = position;
    public Bitmap? Tile { get; } = tile;
}

public class CacheTileLoader : DisposableOnceWithCancel, ITileLoader
{
    private readonly ITileProvider _provider;
    private readonly ILogger<CacheTileLoader>? _logger;
    private readonly HttpClient _httpClient;
    private readonly string _cacheDirectory;
    private readonly Channel<TilePosition> _tileChannel;
    private readonly MemoryCache _cache;
    private readonly Subject<TileLoadedEventArgs> _onLoaded;
    private readonly ConcurrentHashSet<TilePosition> _inProgressHash;
    private readonly ConcurrentHashSet<string> _inProgressUrl;
    private readonly object _sync = new();
    private volatile Bitmap? _emptyBitmap;

    public CacheTileLoader(ILoggerFactory logger, ITileProvider provider)
    {
        _provider = provider;
        _logger = logger.CreateLogger<CacheTileLoader>();
        _onLoaded = new();
        _httpClient = new HttpClient();
        _inProgressHash = new();
        _inProgressUrl = new ConcurrentHashSet<string>();
        _cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", "tiles");
        _cache = new(new MemoryCacheOptions { SizeLimit = 100_000_000, TrackStatistics = true });
        // Создаем директорию кэша, если её нет
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }

        _tileChannel = Channel.CreateBounded<TilePosition>(
            new BoundedChannelOptions(50) { FullMode = BoundedChannelFullMode.DropOldest }
        );
        for (var i = 0; i < 10; i++)
        {
            Task.Run(ProcessQueue);
        }
    }

    private async Task ProcessQueue()
    {
        await foreach (var position in _tileChannel.Reader.ReadAllAsync(DisposeCancel))
        {
            try
            {
                if (_inProgressHash.Add(position) == false)
                {
                    // already in progress => skip
                    continue;
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
                    if (_inProgressUrl.Add(url) == false)
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
                    _inProgressUrl.Remove(url);
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
                _inProgressHash.Remove(position);
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

    public Bitmap GetEmptyBitmap(Color? backgroundColor = null)
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
                var brush = new SolidColorBrush(backgroundColor ?? Colors.Transparent);
                ctx.FillRectangle(brush, new Rect(0, 0, _provider.TileSize, _provider.TileSize));
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

    public Bitmap this[TilePosition position]
    {
        get
        {
            if (_cache.TryGetValue<Bitmap>(position, out var cachedTile))
            {
                return cachedTile ?? GetEmptyBitmap();
            }

            if (_inProgressHash.Contains(position) == false)
            {
                _tileChannel.Writer.TryWrite(position);
            }
            else { }

            return GetEmptyBitmap();
        }
    }

    public Observable<TileLoadedEventArgs> OnLoaded => _onLoaded;
}
