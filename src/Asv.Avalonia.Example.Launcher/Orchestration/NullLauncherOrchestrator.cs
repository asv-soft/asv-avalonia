using Asv.Avalonia.Example.Launcher.Contracts;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Example.Launcher.Orchestration;

public class NullLauncherOrchestrator : ILauncherOrchestrator
{
    public static readonly ILauncherOrchestrator Instance = new NullLauncherOrchestrator();

    public Task<LauncherRunResult> RunAsync(
        LauncherStartOptions options,
        IProgress<LauncherSignal>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(new LauncherRunResult(LauncherExitCode.Success, "Null launcher"));
    }
}
