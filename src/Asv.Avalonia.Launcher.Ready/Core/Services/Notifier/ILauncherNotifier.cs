namespace Asv.Avalonia.Launcher.Ready;

public interface ILauncherNotifier
{
    public Task NotifyReadyAsync(CancellationToken cancellationToken = default);
}
