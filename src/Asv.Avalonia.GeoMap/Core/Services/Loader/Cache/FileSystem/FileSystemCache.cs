using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Asv.Common;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLogger;

namespace Asv.Avalonia.GeoMap;

public class FileSystemCacheConfig : TileCacheConfig
{
    public const string ConfigurationSection = "filesystemcache";

    public string FolderPath { get; set; } = "map";
    public int WriteQueueSize { get; set; } = 100;

    public int WriteParallelThreads { get; set; } = 1;
}

public class FileSystemCache : TileCache
{
    private readonly string _cacheDirectory;
    private readonly ILogger<FileSystemCache> _logger;
    private readonly Lock _syncDir = new();
    private readonly Counter<int> _meterGet;
    private readonly Counter<int> _meterSet;
    private long _fileCount;
    private long _dirSizeInBytes;
    private long _totalHits;
    private long _totalMiss;
    private readonly int _capacitySize;
    private const string TileFileExtension = "png";
    private readonly Channel<Tile> _writerQueue;

    public FileSystemCache(
        IOptions<FileSystemCacheConfig> config,
        ILoggerFactory factory,
        IMeterFactory meterFactory
    )
        : base(config.Value, factory)
    {
        _logger = factory.CreateLogger<FileSystemCache>();
        _cacheDirectory = config.Value.FolderPath;
        _capacitySize = config.Value.SizeLimitKb * 1024;
        if (!Directory.Exists(_cacheDirectory))
        {
            _logger.ZLogInformation($"Create map cache directory: {_cacheDirectory}");
            Directory.CreateDirectory(_cacheDirectory);
        }

        DirectoryHelper.GetDirectorySize(_cacheDirectory, ref _fileCount, ref _dirSizeInBytes);

        var meter = meterFactory.Create(GeoMapMixin.MetricName);
        _meterGet = meter.CreateCounter<int>("cache_file_get");
        _meterSet = meter.CreateCounter<int>("cache_file_set");
        meter.CreateObservableGauge("cache_file_count", () => _fileCount);
        meter.CreateObservableGauge("cache_file_size", () => _dirSizeInBytes / (1024 * 1024), "MB");

        _logger.ZLogInformation(
            $"Map cache directory: {_cacheDirectory}, files: {_fileCount}, size: {_dirSizeInBytes / (1024 * 1024):N} MB"
        );

        _writerQueue = Channel.CreateBounded<Tile>(
            new BoundedChannelOptions(config.Value.WriteQueueSize)
        /*{
            FullMode = BoundedChannelFullMode.DropOldest,
        }*/
        );

        DisposeCancel.Register(() => _writerQueue.Writer.TryComplete());

        for (var i = 0; i < config.Value.WriteParallelThreads; i++)
        {
            Task.Run(WriteQueue);
        }
    }

    private async void WriteQueue()
    {
        try
        {
            await foreach (var tile in _writerQueue.Reader.ReadAllAsync(DisposeCancel))
            {
                var persisted = false;
                try
                {
                    _meterSet.Add(1);
                    var tilePath = GetTileCachePath(tile.Key);

                    var hadFile = File.Exists(tilePath);
                    var oldSize = hadFile ? new FileInfo(tilePath).Length : 0;
                    tile.Save(tilePath);
                    persisted = true;
                    var newSize = new FileInfo(tilePath).Length;
                    if (!hadFile)
                    {
                        Interlocked.Increment(ref _fileCount);
                    }

                    Interlocked.Add(ref _dirSizeInBytes, newSize - oldSize);
                }
                finally
                {
                    if (persisted)
                    {
                        tile.ReleaseCompressedBytes();
                    }

                    tile.Dispose();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // this is normal when dispose
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Error write tile queue:{e.Message}");
        }
    }

    protected override void SetBitmap(TileKey key, Tile? tile)
    {
        /*if (tile == null)
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
        }*/
        if (tile == null)
        {
            return;
        }
        _writerQueue.Writer.TryWrite(tile);
    }

    protected override Tile? GetBitmap(TileKey key)
    {
        _meterGet.Add(1);
        var tilePath = GetTileCachePath(key);
        if (File.Exists(tilePath))
        {
            Interlocked.Increment(ref _totalHits);
            return Tile.Create(key, tilePath);
        }
        else
        {
            Interlocked.Increment(ref _totalMiss);
            return null;
        }
    }

    private string GetTileCachePath(TileKey key)
    {
        var providerName = key.Provider.Info.Id;
        var tileFolder = Path.Combine(_cacheDirectory, providerName, key.Zoom.ToString());
        Directory.CreateDirectory(tileFolder);
        return Path.Combine(tileFolder, $"{key.X}_{key.Y}.{TileFileExtension}");
    }

    public override TileCacheStatistic GetStatistic()
    {
        return new TileCacheStatistic(
            Interlocked.Read(ref _totalHits),
            Interlocked.Read(ref _totalMiss),
            Interlocked.Read(ref _fileCount),
            Interlocked.Read(ref _dirSizeInBytes),
            _capacitySize
        );
    }
}
