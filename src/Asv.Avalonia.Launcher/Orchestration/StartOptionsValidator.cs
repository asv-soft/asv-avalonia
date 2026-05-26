using Asv.Avalonia.Launcher.Contracts;

namespace Asv.Avalonia.Launcher.Orchestration;

public static class StartOptionsValidator
{
    public static string? Validate(LauncherStartOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.TargetPath))
        {
            return "TargetPath argument is required.";
        }

        if (!File.Exists(options.TargetPath))
        {
            return $"Target executable does not exist: {options.TargetPath}";
        }

        if (string.IsNullOrWhiteSpace(options.PipeName))
        {
            return "PipeName argument is required.";
        }

        if (TryValidatePipeNameLength(options.PipeName, out var pipeLengthError) == false)
        {
            return pipeLengthError;
        }

        if (string.IsNullOrWhiteSpace(options.SessionToken))
        {
            return "SessionToken argument is required.";
        }

        if (options.StartupTimeout <= TimeSpan.Zero)
        {
            return $"StartupTimeout should be greater than zero: {options.StartupTimeout}";
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
