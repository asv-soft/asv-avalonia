namespace Asv.Avalonia.Launcher.Ready;

public class NullLauncherNotifier : ILauncherNotifier
{
    public Task NotifyReadyAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
