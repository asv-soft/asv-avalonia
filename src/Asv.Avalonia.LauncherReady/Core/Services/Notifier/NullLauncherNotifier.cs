namespace Asv.Avalonia.Launcher.Ready;

public class NullLauncherNotifier : ILauncherNotifier
{
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    public Task NotifyReadyAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
