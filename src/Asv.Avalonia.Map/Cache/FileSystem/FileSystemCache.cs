using System.Diagnostics.Metrics;
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
    private readonly Counter<int> _meterGet;
    private readonly Counter<int> _meterSet;
    private long _fileCount;
    private long _dirSizeInBytes;
    private long _totalHits;
    private long _totalMiss;
    private readonly int _capacitySize;
    private const string TileFileExtension = "png";

    public FileSystemCache(
        FileSystemCacheConfig config,
        ILoggerFactory factory,
        IMeterFactory meterFactory
    )
        : base(config, factory)
    {
        _logger = factory.CreateLogger<FileSystemCache>();
        _cacheDirectory = config.FolderPath;
        _capacitySize = config.SizeLimitKb * 1024;
        if (!Directory.Exists(_cacheDirectory))
        {
            _logger.ZLogInformation($"Create map cache directory: {_cacheDirectory}");
            Directory.CreateDirectory(_cacheDirectory);
        }
        DirectoryHelper.GetDirectorySize(_cacheDirectory, ref _fileCount, ref _dirSizeInBytes);

        var meter = meterFactory.Create(MapMetric.BaseName);
        _meterGet = meter.CreateCounter<int>("cache_file_get");
        _meterSet = meter.CreateCounter<int>("cache_file_set");
        meter.CreateObservableGauge("cache_file_count", () => _fileCount);
        meter.CreateObservableGauge("cache_file_size", () => _dirSizeInBytes / (1024 * 1024), "MB");

        _logger.ZLogInformation(
            $"Map cache directory: {_cacheDirectory}, files: {_fileCount}, size: {_dirSizeInBytes / (1024 * 1024):N} MB"
        );
    }

    public override Bitmap? this[TileKey key]
    {
        get => _lock.Execute(key, key, GetBitmap);
        set => _lock.Execute(key, key, value, SetBitmap);
    }

    private void SetBitmap(TileKey key, Bitmap? bitmap)
    {
        _meterSet.Add(1);
        var tilePath = GetTileCachePath(key);
        if (bitmap == null)
        {
            var info = new FileInfo(tilePath);
            if (info.Exists)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                _logger.ZLogInformation($"Delete tile file: {tilePath}");
                _dirSizeInBytes -= info.Length;
                _fileCount--;
                File.Delete(tilePath);
            }
        }
        else
        {
            // ReSharper disable once InconsistentlySynchronizedField
            _logger.ZLogInformation($"Create tile file: {tilePath}");
            bitmap.Save(tilePath);
            _fileCount++;
            var info = GetTileCachePath(key);
            _dirSizeInBytes += info.Length;
            // TODO: Check capacity and remove old files
        }
    }

    private Bitmap? GetBitmap(TileKey key)
    {
        _meterGet.Add(1);
        var tilePath = GetTileCachePath(key);
        if (File.Exists(tilePath))
        {
            _totalHits++;
            return new Bitmap(tilePath);
        }
        else
        {
            _totalMiss++;
            return null;
        }
    }

    private string GetTileCachePath(TileKey key)
    {
        var providerName = key.Provider.Info.Id;
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
        return new TileCacheStatistic(
            _totalHits,
            _totalMiss,
            _fileCount,
            _dirSizeInBytes,
            _capacitySize
        );
    }
}
