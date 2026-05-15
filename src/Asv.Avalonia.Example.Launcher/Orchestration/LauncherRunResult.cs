using Asv.Avalonia.Example.Launcher.Contracts;

namespace Asv.Avalonia.Example.Launcher.Orchestration;

public sealed class LauncherRunResult
{
    public LauncherRunResult(
        LauncherExitCode exitCode,
        string message,
        int? targetProcessExitCode = null,
        LauncherSignal? lastSignal = null
    )
    {
        ExitCode = exitCode;
        Message = message;
        TargetProcessExitCode = targetProcessExitCode;
        LastSignal = lastSignal;
    }

    public LauncherExitCode ExitCode { get; }
    public string Message { get; }
    public int? TargetProcessExitCode { get; }
    public LauncherSignal? LastSignal { get; }
}
