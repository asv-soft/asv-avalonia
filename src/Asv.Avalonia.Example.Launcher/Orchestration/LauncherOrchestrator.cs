using System.Diagnostics;
using Asv.Avalonia.Example.Launcher.Contracts;
using Asv.Avalonia.Example.Launcher.Ipc;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Example.Launcher.Orchestration;

public sealed class LauncherOrchestrator : ILauncherOrchestrator
{
    public const string LauncherPipeArg = LauncherCommandLineArguments.LauncherPipeArg;
    public const string LauncherTokenArg = LauncherCommandLineArguments.LauncherTokenArg;

    private readonly ILauncherSignalServerFactory _signalServerFactory;

    public LauncherOrchestrator(ILauncherSignalServerFactory? signalServerFactory = null)
    {
        _signalServerFactory = signalServerFactory ?? new NamedPipeLauncherSignalServerFactory();
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

        var validationError = Validate(options);
        if (validationError != null)
        {
            return validationError;
        }

        progress?.Report(
            new LauncherSignal(LauncherSignalType.Progress, "Starting target process...", 0.2)
        );

        var processStartInfo = CreateStartInfo(options);
        using var process = new Process();
        process.StartInfo = processStartInfo;

        try
        {
            if (!process.Start())
            {
                return new LauncherRunResult(
                    LauncherExitCode.TargetStartFailed,
                    "Failed to start target process."
                );
            }
        }
        catch (Exception ex)
        {
            return new LauncherRunResult(
                LauncherExitCode.TargetStartFailed,
                $"Failed to start target process: {ex.Message}"
            );
        }

        progress?.Report(
            new LauncherSignal(
                LauncherSignalType.Progress,
                "Waiting for target READY signal...",
                0.3
            )
        );

        var processExitTask = process.WaitForExitAsync(cancellationToken);
        var timeoutTask = Task.Delay(options.StartupTimeout, cancellationToken);

        while (true)
        {
            var signalTask = WaitForSignalAsync(options, cancellationToken);
            var completedTask = await Task.WhenAny(signalTask, processExitTask, timeoutTask)
                .ConfigureAwait(false);

            if (ReferenceEquals(completedTask, timeoutTask))
            {
                return new LauncherRunResult(
                    LauncherExitCode.StartupTimeout,
                    $"Startup timeout ({options.StartupTimeout})."
                );
            }

            if (ReferenceEquals(completedTask, processExitTask))
            {
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
            catch (Exception ex)
            {
                return new LauncherRunResult(LauncherExitCode.IpcError, $"IPC error: {ex.Message}");
            }

            progress?.Report(signalMessage.Signal);

            switch (signalMessage.Signal.Type)
            {
                case LauncherSignalType.Progress:
                    continue;
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

    private static ProcessStartInfo CreateStartInfo(LauncherStartOptions options)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = options.TargetPath,
            WorkingDirectory =
                Path.GetDirectoryName(options.TargetPath) ?? Environment.CurrentDirectory,
        };

        foreach (var targetArg in options.TargetArgs)
        {
            startInfo.ArgumentList.Add(targetArg);
        }

        startInfo.ArgumentList.Add(LauncherPipeArg);
        startInfo.ArgumentList.Add(options.PipeName);
        startInfo.ArgumentList.Add(LauncherTokenArg);
        startInfo.ArgumentList.Add(options.SessionToken);

        return startInfo;
    }

    private static LauncherRunResult? Validate(LauncherStartOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.TargetPath))
        {
            return new LauncherRunResult(
                LauncherExitCode.InvalidArguments,
                "TargetPath argument is required."
            );
        }

        if (!File.Exists(options.TargetPath))
        {
            return new LauncherRunResult(
                LauncherExitCode.InvalidArguments,
                $"Target executable does not exist: {options.TargetPath}"
            );
        }

        if (string.IsNullOrWhiteSpace(options.PipeName))
        {
            return new LauncherRunResult(
                LauncherExitCode.InvalidArguments,
                "PipeName argument is required."
            );
        }

        if (TryValidatePipeNameLength(options.PipeName, out var pipeLengthError) == false)
        {
            return new LauncherRunResult(LauncherExitCode.InvalidArguments, pipeLengthError);
        }

        if (string.IsNullOrWhiteSpace(options.SessionToken))
        {
            return new LauncherRunResult(
                LauncherExitCode.InvalidArguments,
                "SessionToken argument is required."
            );
        }

        if (options.StartupTimeout <= TimeSpan.Zero)
        {
            return new LauncherRunResult(
                LauncherExitCode.InvalidArguments,
                $"StartupTimeout should be greater than zero: {options.StartupTimeout}"
            );
        }

        return null;
    }

    private static bool TryValidatePipeNameLength(string pipeName, out string error)
    {
        error = string.Empty;

        // On Unix, .NET named pipes are backed by Unix domain sockets with max path length 104.
        if (OperatingSystem.IsWindows())
        {
            return true;
        }

        const int maxSocketPathLength = 104;
        const string coreFxPipePrefix = "CoreFxPipe_";
        var estimatedSocketPathLength =
            Path.GetTempPath().Length + coreFxPipePrefix.Length + pipeName.Length;
        if (estimatedSocketPathLength <= maxSocketPathLength)
        {
            return true;
        }

        var maxPipeNameLength =
            maxSocketPathLength - Path.GetTempPath().Length - coreFxPipePrefix.Length;
        if (maxPipeNameLength < 1)
        {
            maxPipeNameLength = 1;
        }

        error =
            $"Pipe name is too long for this platform/temp path. "
            + $"Current length: {pipeName.Length}, max allowed: {maxPipeNameLength}.";
        return false;
    }
}
