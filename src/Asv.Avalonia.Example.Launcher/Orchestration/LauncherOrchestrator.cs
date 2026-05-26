using Asv.Avalonia.Example.Launcher.Contracts;
using Asv.Avalonia.Example.Launcher.Ipc;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Example.Launcher.Orchestration;

public sealed class LauncherOrchestrator : ILauncherOrchestrator
{
    private readonly ProcessStartInfoFactory _startInfoFactory;
    private readonly TargetProcessRunner _targetProcessRunner;
    private readonly StartupMonitor _startupMonitor;

    public LauncherOrchestrator()
    {
        var signalServerFactory = new NamedPipeLauncherSignalServerFactory();

        _startInfoFactory = new ProcessStartInfoFactory(() => Environment.ProcessPath);
        _targetProcessRunner = new TargetProcessRunner();
        _startupMonitor = new StartupMonitor(signalServerFactory, _targetProcessRunner);
    }

    public async Task<LauncherRunResult> RunAsync(
        LauncherStartOptions options,
        IProgress<LauncherSignal>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        progress?.Report(
            new LauncherSignal(LauncherSignalType.Progress, "Validating options...", 0.1)
        );

        var validationError = StartOptionsValidator.Validate(options);
        if (validationError is not null)
        {
            return new LauncherRunResult(LauncherExitCode.InvalidArguments, validationError);
        }

        progress?.Report(
            new LauncherSignal(LauncherSignalType.Progress, "Starting target process...", 0.2)
        );

        var startInfo = _startInfoFactory.Create(options);
        var startResult = _targetProcessRunner.Start(startInfo);
        if (startResult.Process is null)
        {
            return new LauncherRunResult(
                LauncherExitCode.TargetStartFailed,
                startResult.ErrorMessage ?? "Failed to start target process."
            );
        }

        using var process = startResult.Process;

        progress?.Report(
            new LauncherSignal(
                LauncherSignalType.Progress,
                "Waiting for target READY signal...",
                0.3
            )
        );

        return await _startupMonitor
            .WaitForReadyAsync(process, options, progress, cancellationToken)
            .ConfigureAwait(false);
    }
}
