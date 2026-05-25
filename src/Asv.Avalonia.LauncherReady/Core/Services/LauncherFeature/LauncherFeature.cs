using Asv.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Launcher.Ready;

internal sealed class LauncherFeature : IHostedService
{
    private readonly IShellHost _shellHost;
    private readonly ILauncherNotifier _notifier;
    private readonly ILogger<LauncherFeature> _logger;
    private volatile int _readyNotificationSent;
    private IDisposable? _sub1;

    public LauncherFeature(
        IShellHost shellHost,
        ILauncherNotifier notifier,
        ILogger<LauncherFeature> logger
    )
    {
        ArgumentNullException.ThrowIfNull(shellHost);
        ArgumentNullException.ThrowIfNull(notifier);
        ArgumentNullException.ThrowIfNull(logger);

        _shellHost = shellHost;
        _notifier = notifier;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sub1 = _shellHost.ExecuteNowOrWhenShellLoaded(
            (_, _) =>
            {
                NotifyReadyOnceAsync(cancellationToken)
                    .SafeFireAndForget(ex =>
                        _logger.LogError(ex, "Failed to send launcher READY signal.")
                    );
            }
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        _sub1?.Dispose();
        return Task.CompletedTask;
    }

    private async Task NotifyReadyOnceAsync(CancellationToken cancel)
    {
        if (Interlocked.CompareExchange(ref _readyNotificationSent, 1, 0) != 0)
        {
            return;
        }

        await _notifier.NotifyReadyAsync(cancel).ConfigureAwait(false);
    }
}
