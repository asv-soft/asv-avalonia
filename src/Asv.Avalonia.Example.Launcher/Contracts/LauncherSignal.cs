namespace Asv.Avalonia.Example.Launcher.Contracts;

public sealed record LauncherSignal
{
    public LauncherSignal(LauncherSignalType type, string? message = null, double? progress = null)
    {
        Type = type;
        Message = message;
        Progress = progress;
    }

    public LauncherSignalType Type { get; }
    public string? Message { get; }
    public double? Progress { get; }
}
