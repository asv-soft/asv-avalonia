using Asv.Common;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia.Map;

public class FileSystemCacheConfig : TileCacheConfig
{
    public string FolderPath { get; set; } = "map";
}

public class FileSystemCache : TileCache
{
    private readonly string _cacheDirectory;
    private readonly ILogger<FileSystemCache> _logger;
    private readonly LockByKeyExecutor<TileKey> _lock = new();
    private readonly object _syncDir = new();
    private const string TileFileExtension = "png";

    public FileSystemCache(FileSystemCacheConfig config, ILoggerFactory factory)
        : base(config, factory)
    {
        _logger = factory.CreateLogger<FileSystemCache>();
        _cacheDirectory = config.FolderPath;
        if (!Directory.Exists(_cacheDirectory))
        {
            _logger.ZLogInformation($"Create map cache directory: {_cacheDirectory}");
            Directory.CreateDirectory(_cacheDirectory);
        }
        // TODO: limit size of file cache
    }

    public override Bitmap? this[TileKey key]
    {
        get => _lock.Execute(key, key, GetBitmap);
        set => _lock.Execute(key, key, value, SetBitmap);
    }

    private void SetBitmap(TileKey key, Bitmap? bitmap)
    {
        var tilePath = GetTileCachePath(key);
        if (bitmap == null)
        {
            if (File.Exists(tilePath))
            {
                // ReSharper disable once InconsistentlySynchronizedField
                _logger.ZLogInformation($"Delete tile cache: {tilePath}");
                File.Delete(tilePath);
            }
        }
        else
        {
            // ReSharper disable once InconsistentlySynchronizedField
            _logger.ZLogInformation($"Save tile cache: {tilePath}");
            bitmap.Save(tilePath);
        }
    }

    private Bitmap? GetBitmap(TileKey key)
    {
        var tilePath = GetTileCachePath(key);
        return File.Exists(tilePath) ? new Bitmap(tilePath) : null;
    }

    private string GetTileCachePath(TileKey key)
    {
        var providerName = key.Provider;
        var tileFolder = Path.Combine(_cacheDirectory, providerName, key.Zoom.ToString());
        if (Directory.Exists(tileFolder) == false)
        {
            lock (_syncDir)
            {
                if (Directory.Exists(tileFolder) == false)
                {
                    _logger.ZLogInformation($"Create tile cache directory: {tileFolder}");
                    Directory.CreateDirectory(tileFolder);
                }
            }
        }
        return Path.Combine(tileFolder, $"{key.X}_{key.Y}.{TileFileExtension}");
    }

    public override TileCacheStatistic GetStatistic()
    {
        return base.GetStatistic();
    }
}
