using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia.Map;

public interface ITileLoader
{
    Task<Bitmap?> GetTileAsync(
        TilePosition position,
        ITileProvider provider,
        CancellationToken cancel
    );
}

public class OnlineTileLoader : ITileLoader
{
    private readonly ILogger<OnlineTileLoader>? _logger;
    private static readonly HttpClient HttpClient = new();
    private readonly string _cacheDirectory;

    public OnlineTileLoader(ILoggerFactory logger)
    {
        _logger = logger?.CreateLogger<OnlineTileLoader>();
        _cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", "tiles");

        // Создаем директорию кэша, если её нет
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    public async Task<Bitmap?> GetTileAsync(
        TilePosition position,
        ITileProvider provider,
        CancellationToken cancel
    )
    {
        provider = provider ?? throw new ArgumentNullException(nameof(provider));

        string tilePath = GetTileCachePath(position, provider);

        // Проверяем, есть ли тайл в кэше
        if (File.Exists(tilePath))
        {
            try
            {
                using var stream = File.OpenRead(tilePath);
                return new Bitmap(stream);
            }
            catch (Exception ex)
            {
                _logger?.ZLogWarning(ex, $"Failed to read cached tile {position}: {ex.Message}");
            }
        }

        // Если нет, загружаем с сервера
        var url = provider.GetTileUrl(position);
        if (url == null)
        {
            return null;
        }

        try
        {
            var img = await HttpClient.GetByteArrayAsync(url, cancel).ConfigureAwait(false);

            // Сохраняем в кэш
            await File.WriteAllBytesAsync(tilePath, img, cancel);

            return new Bitmap(new MemoryStream(img));
        }
        catch (Exception ex)
        {
            _logger?.ZLogError(ex, $"Failed to load tile {position}: {ex.Message}");
            return null;
        }
    }

    private string GetTileCachePath(TilePosition position, ITileProvider provider)
    {
        // Генерируем путь на основе провайдера, zoom, X и Y координат тайла
        string providerName = provider.GetType().Name;
        string tileFolder = Path.Combine(_cacheDirectory, providerName, position.Zoom.ToString());

        if (!Directory.Exists(tileFolder))
        {
            Directory.CreateDirectory(tileFolder);
        }

        return Path.Combine(tileFolder, $"{position.X}_{position.Y}.png");
    }
}
