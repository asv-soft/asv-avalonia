using Asv.Common;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia.Map;

public class TileCacheConfig
{
    /// <summary>
    /// Gets or sets the duration, in seconds, after which cached tiles are considered expired and eligible for removal.
    /// A value of 0 or less disables automatic expiration of cached tiles.
    /// </summary>
    public double ExpirationAfterSec { get; set; } = 30 * 60;

    /// <summary>
    /// Gets or sets the interval, in seconds, at which cache statistics are printed to the log.
    /// A value of 0 or less disables the logging of statistics.
    /// </summary>
    public double PrintStatisticsToLogPeriodSec { get; set; } = 30;

    /// <summary>
    /// Gets or sets the frequency, in seconds, at which the cache scans for expired entries.
    /// </summary>
    public int ExpirationScanFrequencySec { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum size limit for the cache in kilobytes (KB).
    /// </summary>
    public int SizeLimitKb { get; set; } = 100_000;

    /// <summary>
    /// Gets or sets the amount the cache is compacted by when the maximum size is exceeded.
    /// </summary>
    public double CompactionPercentage { get; set; } = 0.5;

    protected virtual IEnumerable<string> GetOptions()
    {
        if (PrintStatisticsToLogPeriodSec <= 0)
        {
            yield return "print to log: disabled";
        }
        else
        {
            yield return $"scan every {PrintStatisticsToLogPeriodSec:F1} sec";
        }

        yield return $"scan every {ExpirationScanFrequencySec:F1} sec";
        yield return $"max size {SizeLimitKb:N} kb";
        yield return $"compaction {CompactionPercentage:P0}";
    }

    public sealed override string ToString()
    {
        return string.Join(", ", GetOptions());
    }
}

public abstract class TileCache : AsyncDisposableOnce, ITileCache
{
    private readonly IDisposable? _timer;

    protected TileCache(TileCacheConfig config, ILoggerFactory factory)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(factory);
        var logger = factory.CreateLogger<TileCache>();
        if (config.PrintStatisticsToLogPeriodSec > 0)
        {
            _timer = Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(config.PrintStatisticsToLogPeriodSec))
                .Subscribe(_ => logger.ZLogInformation($"Stat: {GetStatistic()}"));
        }
    }

    public abstract Bitmap? this[TileKey key] { get; set; }

    public virtual TileCacheStatistic GetStatistic()
    {
        return TileCacheStatistic.Empty;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_timer is IAsyncDisposable timerAsyncDisposable)
        {
            await timerAsyncDisposable.DisposeAsync();
        }
        else
        {
            _timer?.Dispose();
        }

        await base.DisposeAsyncCore();
    }
}
