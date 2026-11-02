using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Asv.Common;
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

public class FileSystemCache : TileCache, IPersistentTileCache
{
    private readonly string _cacheDirectory;
    private readonly ILogger<FileSystemCache> _logger;
    private readonly SemaphoreSlim _writeSync = new(1, 1);
    private readonly Task[] _writers;
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
        IMeterFactory meterFactory,
        IAppPath appPath
    )
        : base(config.Value, factory)
    {
        ArgumentNullException.ThrowIfNull(appPath);

        _logger = factory.CreateLogger<FileSystemCache>();
        _cacheDirectory = appPath.GetAppPathFolder(config.Value.FolderPath);
        _capacitySize = config.Value.SizeLimitKb * 1024;
        if (!Directory.Exists(_cacheDirectory))
        {
            _logger.ZLogInformation($"Create map cache directory: {_cacheDirectory}");
            Directory.CreateDirectory(_cacheDirectory);
        }

        DirectoryHelper.GetDirectorySize(_cacheDirectory, ref _fileCount, ref _dirSizeInBytes);

        var meter = meterFactory.Create(GeoMapRegistrations.MetricName);
        _meterGet = meter.CreateCounter<int>("cache_file_get");
        _meterSet = meter.CreateCounter<int>("cache_file_set");
        meter.CreateObservableGauge("cache_file_count", () => _fileCount);
        meter.CreateObservableGauge("cache_file_size", () => _dirSizeInBytes / (1024 * 1024), "MB");

        _logger.ZLogInformation(
            $"Map cache directory: {_cacheDirectory}, files: {_fileCount}, size: {_dirSizeInBytes / (1024 * 1024):N} MB"
        );

        _writerQueue = Channel.CreateBounded<Tile>(
            new BoundedChannelOptions(Math.Max(1, config.Value.WriteQueueSize))
            {
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        _writers = new Task[Math.Max(1, config.Value.WriteParallelThreads)];
        for (var i = 0; i < _writers.Length; i++)
        {
            _writers[i] = Task.Run(WriteQueueAsync);
        }
    }

    private async Task WriteQueueAsync()
    {
        // Complete and drain the queue during disposal; each entry owns one reference.
        await foreach (var tile in _writerQueue.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            WriteAndRelease(tile);
        }
    }

    private void WriteAndRelease(Tile tile)
    {
        try
        {
            WriteTile(tile, CancellationToken.None, releaseCompressed: true);
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Error writing tile cache:{e.Message}");
        }
        finally
        {
            tile.Dispose();
        }
    }

    protected override void SetBitmap(TileKey key, Tile? tile)
    {
        if (tile == null)
        {
            return;
        }

        if (IsDisposed)
        {
            tile.Dispose();
            return;
        }

        if (!_writerQueue.Writer.TryWrite(tile))
        {
            // The synchronous cache API cannot await capacity. Persist on this loader
            // thread instead, bounding memory without dropping the transferred reference.
            WriteAndRelease(tile);
        }
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
        return Path.Combine(tileFolder, $"{key.X}_{key.Y}.{TileFileExtension}");
    }

    public bool Contains(TileKey key)
    {
        ThrowIfDisposed();
        return File.Exists(GetTileCachePath(key));
    }

    public ValueTask SaveAsync(Tile tile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tile);
        ThrowIfDisposed();
        return new ValueTask(Task.Run(() => WriteTile(tile, cancellationToken), cancellationToken));
    }

    private void WriteTile(
        Tile tile,
        CancellationToken cancellationToken,
        bool releaseCompressed = false
    )
    {
        _writeSync.Wait(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tilePath = GetTileCachePath(tile.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(tilePath)!);
            var temporaryPath = tilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                tile.Save(temporaryPath);
                using (
                    var stream = new FileStream(
                        temporaryPath,
                        FileMode.Open,
                        FileAccess.Write,
                        FileShare.None
                    )
                )
                {
                    stream.Flush(flushToDisk: true);
                }

                var hadFile = File.Exists(tilePath);
                var oldSize = hadFile ? new FileInfo(tilePath).Length : 0;
                File.Move(temporaryPath, tilePath, overwrite: true);
                var newSize = new FileInfo(tilePath).Length;
                if (!hadFile)
                {
                    Interlocked.Increment(ref _fileCount);
                }

                Interlocked.Add(ref _dirSizeInBytes, newSize - oldSize);
                _meterSet.Add(1);
                if (releaseCompressed)
                {
                    tile.ReleaseCompressedBytes();
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
        finally
        {
            _writeSync.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writerQueue.Writer.TryComplete();
            Task.WaitAll(_writers);
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        _writerQueue.Writer.TryComplete();
        await Task.WhenAll(_writers).ConfigureAwait(false);
        await base.DisposeAsyncCore().ConfigureAwait(false);
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
