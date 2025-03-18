using System.Diagnostics.Metrics;
using Asv.Common;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia.Map;

public class MemoryTileCacheConfig : TileCacheConfig
{
    public double ExpirationAfterSec { get; set; } = 30 * 60;

    protected override IEnumerable<string> GetOptions()
    {
        foreach (var option in base.GetOptions())
        {
            yield return option;
        }

        yield return $"expire after {ExpirationAfterSec:F1} sec";
    }
}

public class MemoryTileCache : TileCache
{
    private readonly MemoryCache _cache;
    private readonly ILogger<MemoryTileCache> _logger;
    private readonly TimeSpan _expirationAfter;
    private readonly int _sizeLimitBytes;
    private readonly Counter<int> _meterGet;
    private readonly Counter<int> _meterSet;
    private readonly Counter<int> _meterClear;
    private readonly ObservableGauge<int> _meterCount;
    private readonly ObservableGauge<long> _meterSize;

    public MemoryTileCache(
        MemoryTileCacheConfig config,
        ILoggerFactory factory,
        IMeterFactory meterFactory
    )
        : base(config, factory)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(factory);

        _sizeLimitBytes = config.SizeLimitKb * 1024;

        _expirationAfter = TimeSpan.FromSeconds(config.ExpirationAfterSec);
        _logger = factory.CreateLogger<MemoryTileCache>();
        _logger.ZLogInformation($"{nameof(MemoryTileCache)} created with {config}");
        _cache = new MemoryCache(
            new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMicroseconds(
                    config.ExpirationScanFrequencySec
                ),
                SizeLimit = _sizeLimitBytes,
                CompactionPercentage = config.CompactionPercentage,
                TrackLinkedCacheEntries = false,
                TrackStatistics = true,
            }
        );

        var meter = meterFactory.Create(MapMetric.BaseName);
        _meterGet = meter.CreateCounter<int>("cache_memory_get");
        _meterSet = meter.CreateCounter<int>("cache_memory_set");
        _meterClear = meter.CreateCounter<int>("cache_memory_evict");
        _meterCount = meter.CreateObservableGauge("cache_memory_count", () => _cache.Count);
        _meterSize = meter.CreateObservableGauge(
            "cache_memory_size",
            () => (_cache.GetCurrentStatistics()?.CurrentEstimatedSize ?? 0L) / (1024 * 1024),
            "MB"
        );
    }

    public override Bitmap? this[TileKey key]
    {
        get
        {
            _meterGet.Add(1);
            return _cache.Get<Bitmap>(key);
        }
        set
        {
            if (value == null)
            {
                _cache.Remove(key);
                return;
            }
            _meterSet.Add(1);
            _cache.Set(key, value, CreateOptions(value));
        }
    }

    public override TileCacheStatistic GetStatistic()
    {
        var stat = _cache.GetCurrentStatistics();
        return stat == null
            ? base.GetStatistic()
            : new TileCacheStatistic(
                stat.TotalHits,
                stat.TotalMisses,
                stat.CurrentEntryCount,
                stat.CurrentEstimatedSize ?? 0,
                _sizeLimitBytes
            );
    }

    private MemoryCacheEntryOptions CreateOptions(Bitmap value)
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = null,
            AbsoluteExpirationRelativeToNow = null,
            SlidingExpiration = _expirationAfter,
            Priority = CacheItemPriority.Low,
            Size = value.PixelSize.Width * value.PixelSize.Height * 4,
        }.RegisterPostEvictionCallback(DisposeAfterEviction);
    }

    private void DisposeAfterEviction(
        object key,
        object? value,
        EvictionReason reason,
        object? state
    )
    {
        _logger.ZLogTrace($"Evict {key} {reason}");
        if (value is IDisposable disposable)
        {
            _meterClear.Add(1);
            disposable.Dispose();
        }
    }

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cache.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        _cache.Dispose();

        await base.DisposeAsyncCore();
    }

    #endregion
}
