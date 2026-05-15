namespace Asv.Avalonia.Example.Launcher.Contracts;

public enum LauncherExitCode
{
    Success = 0,
    StartupTimeout = 10,
    TargetStartFailed = 11,
    TargetExitedBeforeReady = 12,
    IpcError = 13,
    InvalidArguments = 14,
    UnexpectedError = 99,
}
