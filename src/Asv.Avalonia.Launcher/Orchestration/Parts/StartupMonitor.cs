using System.Diagnostics;
using Asv.Avalonia.Launcher.Api;
using Asv.Avalonia.Launcher.Contracts;
using Asv.Avalonia.Launcher.Ipc;

namespace Asv.Avalonia.Launcher.Orchestration;

public sealed class StartupMonitor
{
    private readonly ILauncherSignalServerFactory _signalServerFactory;
    private readonly TargetProcessRunner _targetProcessRunner;

    public StartupMonitor(
        ILauncherSignalServerFactory signalServerFactory,
        TargetProcessRunner targetProcessRunner
    )
    {
        ArgumentNullException.ThrowIfNull(signalServerFactory);
        ArgumentNullException.ThrowIfNull(targetProcessRunner);

        _signalServerFactory = signalServerFactory;
        _targetProcessRunner = targetProcessRunner;
    }

    public async Task<LauncherRunResult> WaitForReadyAsync(
        Process process,
        LauncherStartOptions options,
        IProgress<LauncherSignal>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentNullException.ThrowIfNull(options);

        using var startupTimeoutCts = new CancellationTokenSource(options.StartupTimeout);
        using var waitCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            startupTimeoutCts.Token
        );
        var processExitTask = process.WaitForExitAsync(cancellationToken);

        while (true)
        {
            var signalTask = WaitForSignalAsync(options, waitCts.Token);
            var completedTask = await Task.WhenAny(signalTask, processExitTask)
                .ConfigureAwait(false);

            if (ReferenceEquals(completedTask, processExitTask))
            {
                await CancelSignalWaitAsync(signalTask, waitCts).ConfigureAwait(false);
                await processExitTask.ConfigureAwait(false);
                return new LauncherRunResult(
                    LauncherExitCode.TargetExitedBeforeReady,
                    "Target process exited before READY signal.",
                    process.ExitCode
                );
            }

            LauncherIpcMessage signalMessage;
            try
            {
                signalMessage = await signalTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
                when (startupTimeoutCts.IsCancellationRequested
                    && !cancellationToken.IsCancellationRequested
                )
            {
                await _targetProcessRunner
                    .TerminateAsync(process, cancellationToken)
                    .ConfigureAwait(false);
                return new LauncherRunResult(
                    LauncherExitCode.StartupTimeout,
                    $"Startup timeout ({options.StartupTimeout}). Target process did not report READY."
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new LauncherRunResult(LauncherExitCode.IpcError, $"IPC error: {ex.Message}");
            }

            progress?.Report(signalMessage.Signal);

            var result = TryCreateTerminalResult(signalMessage, process);
            if (result is not null)
            {
                return result;
            }
        }
    }

    private async Task<LauncherIpcMessage> WaitForSignalAsync(
        LauncherStartOptions options,
        CancellationToken cancellationToken
    )
    {
        await using var server = _signalServerFactory.Create(
            options.PipeName,
            options.SessionToken
        );
        return await server.WaitForSignalAsync(cancellationToken).ConfigureAwait(false);
    }

    private static LauncherRunResult? TryCreateTerminalResult(
        LauncherIpcMessage signalMessage,
        Process process
    )
    {
        switch (signalMessage.Signal.Type)
        {
            case LauncherSignalType.Progress:
                return null;
            case LauncherSignalType.Ready:
                return new LauncherRunResult(
                    LauncherExitCode.Success,
                    "Target process reported READY.",
                    process.HasExited ? process.ExitCode : null,
                    signalMessage.Signal
                );
            case LauncherSignalType.Error:
                return new LauncherRunResult(
                    LauncherExitCode.TargetStartFailed,
                    signalMessage.Signal.Message ?? "Target process reported ERROR.",
                    process.HasExited ? process.ExitCode : null,
                    signalMessage.Signal
                );
            default:
                return new LauncherRunResult(
                    LauncherExitCode.IpcError,
                    $"Unsupported launcher signal type: {signalMessage.Signal.Type}.",
                    process.HasExited ? process.ExitCode : null,
                    signalMessage.Signal
                );
        }
    }

    private static async Task CancelSignalWaitAsync(
        Task<LauncherIpcMessage> signalTask,
        CancellationTokenSource cancellationTokenSource
    )
    {
        if (!signalTask.IsCompleted)
        {
            await cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        }

        try
        {
            await signalTask.WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch
        {
            // The signal wait is being canceled because another terminal condition already won.
        }
    }
}
