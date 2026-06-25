using System.Diagnostics.CodeAnalysis;

namespace Asv.Avalonia.Launcher.Ready;

public interface ILauncherNotifier
{
    [RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    public Task NotifyReadyAsync(CancellationToken cancellationToken = default);
}
