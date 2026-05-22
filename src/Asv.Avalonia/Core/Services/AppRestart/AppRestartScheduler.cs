using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

internal sealed class AppRestartScheduler(ILoggerFactory loggerFactory) : IAppRestartScheduler
{
    private readonly ILogger<AppRestartScheduler> _logger =
        loggerFactory.CreateLogger<AppRestartScheduler>();

    private int _isScheduled;

    public bool IsScheduled => Volatile.Read(ref _isScheduled) != 0;

    public void Schedule()
    {
        if (Interlocked.Exchange(ref _isScheduled, 1) != 0)
        {
            return;
        }

        _logger.LogInformation("Application restart is scheduled.");
    }

    public void Cancel()
    {
        if (Interlocked.Exchange(ref _isScheduled, 0) == 0)
        {
            return;
        }

        _logger.LogInformation("Application restart is canceled.");
    }
}
