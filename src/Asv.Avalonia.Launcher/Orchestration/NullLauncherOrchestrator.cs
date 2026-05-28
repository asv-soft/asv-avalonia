using Asv.Avalonia.Launcher.Api;
using Asv.Avalonia.Launcher.Contracts;

namespace Asv.Avalonia.Launcher.Orchestration;

public sealed class NullLauncherOrchestrator : ILauncherOrchestrator
{
    public static readonly ILauncherOrchestrator Instance = new NullLauncherOrchestrator();

    private NullLauncherOrchestrator() { }

    public Task<LauncherRunResult> RunAsync(
        LauncherStartOptions options,
        IProgress<LauncherSignal>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(new LauncherRunResult(LauncherExitCode.Success, "Null launcher"));
    }
}
