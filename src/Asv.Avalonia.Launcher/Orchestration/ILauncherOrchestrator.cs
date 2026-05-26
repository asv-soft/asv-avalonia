using Asv.Avalonia.Launcher.Api;
using Asv.Avalonia.Launcher.Contracts;

namespace Asv.Avalonia.Launcher.Orchestration;

public interface ILauncherOrchestrator
{
    Task<LauncherRunResult> RunAsync(
        LauncherStartOptions options,
        IProgress<LauncherSignal>? progress = null,
        CancellationToken cancellationToken = default
    );
}
