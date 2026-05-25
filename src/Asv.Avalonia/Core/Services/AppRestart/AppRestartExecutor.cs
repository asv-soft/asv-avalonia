using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

internal sealed class AppRestartExecutor(
    IAppRestartScheduler scheduler,
    IAppRestartFeature feature,
    ILoggerFactory loggerFactory
) : IHostedLifecycleService
{
    private readonly ILogger<AppRestartExecutor> _logger =
        loggerFactory.CreateLogger<AppRestartExecutor>();

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        if (!scheduler.IsScheduled)
        {
            return Task.CompletedTask;
        }

        try
        {
            feature.Restart();
            _logger.LogInformation("Scheduled application restart feature was invoked.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute scheduled application restart.");
        }

        return Task.CompletedTask;
    }
}
