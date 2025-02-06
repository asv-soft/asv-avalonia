using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia.Map;

public interface ITileLoader
{
    Task<Bitmap?> GetTileAsync(TileKey key, ITileProvider provider, CancellationToken cancel);
}

public class OnlineTileLoader : ITileLoader
{
    private readonly ILogger<OnlineTileLoader>? _logger;
    private static readonly HttpClient _httpClient = new();

    public OnlineTileLoader(ILoggerFactory logger)
    {
        _logger = logger?.CreateLogger<OnlineTileLoader>();
    }

    public async Task<Bitmap?> GetTileAsync(
        TileKey key,
        ITileProvider provider,
        CancellationToken cancel
    )
    {
        provider = provider ?? throw new ArgumentNullException(nameof(provider));
        var url = provider.GetTileUrl(key);
        if (url == null)
        {
            return null;
        }
        try
        {
            var img = await _httpClient.GetByteArrayAsync(url, cancel).ConfigureAwait(false);
            return new Bitmap(new MemoryStream(img));
        }
        catch (Exception ex)
        {
            _logger?.ZLogError(ex, $"Failed to load tile {key}: {ex.Message}", key);
            return null;
        }
    }
}
