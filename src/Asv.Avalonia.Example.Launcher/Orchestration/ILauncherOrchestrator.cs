using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Launcher.Contracts;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Example.Launcher.Orchestration;

public interface ILauncherOrchestrator
{
    Task<LauncherRunResult> RunAsync(
        LauncherStartOptions options,
        IProgress<LauncherSignal>? progress = null,
        CancellationToken cancellationToken = default
    );
}
