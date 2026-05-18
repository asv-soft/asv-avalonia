using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Launcher.Ready;

internal sealed class LauncherReadyShellExtension(ILogger<LauncherReadyShellExtension> logger)
    : IExtensionFor<IShell>
{
    private int _readyNotificationSent;

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(contextDispose);

        if (
            LauncherReadyNotifier.TryReadEndpoint(
                Environment.GetCommandLineArgs(),
                out var endpoint
            ) == false
        )
        {
            return;
        }

        if (
            Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            return;
        }

        var mainWindow = lifetime.MainWindow;
        if (mainWindow == null)
        {
            return;
        }

        if (mainWindow.IsVisible)
        {
            _ = NotifyReadyOnceAsync(endpoint);
            return;
        }

        void OnOpened(object? sender, EventArgs e)
        {
            mainWindow.Opened -= OnOpened;
            _ = NotifyReadyOnceAsync(endpoint);
        }

        mainWindow.Opened += OnOpened;
        contextDispose.Add(Disposable.Create(() => mainWindow.Opened -= OnOpened));
        logger.LogInformation("Launcher is ready.");
    }

    private async Task NotifyReadyOnceAsync(LauncherReadyEndpoint endpoint)
    {
        if (Interlocked.CompareExchange(ref _readyNotificationSent, 1, 0) != 0)
        {
            return;
        }

        try
        {
            await LauncherReadyNotifier.NotifyReadyAsync(endpoint).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send launcher READY signal.");
        }
    }
}
