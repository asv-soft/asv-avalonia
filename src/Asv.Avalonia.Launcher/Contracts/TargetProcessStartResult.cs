using System.Diagnostics;

namespace Asv.Avalonia.Launcher.Contracts;

public sealed class TargetProcessStartResult
{
    private TargetProcessStartResult(Process? process, string? errorMessage)
    {
        Process = process;
        ErrorMessage = errorMessage;
    }

    public Process? Process { get; }
    public string? ErrorMessage { get; }

    public static TargetProcessStartResult Started(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);
        return new TargetProcessStartResult(process, null);
    }

    public static TargetProcessStartResult Failed(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new TargetProcessStartResult(null, errorMessage);
    }
}
