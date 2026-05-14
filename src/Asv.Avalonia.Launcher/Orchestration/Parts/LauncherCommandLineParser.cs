using Asv.Avalonia.Launcher.Api;
using Asv.Avalonia.Launcher.Contracts;

namespace Asv.Avalonia.Launcher.Orchestration;

public static class LauncherCommandLineParser
{
    public static bool TryParse(
        IReadOnlyList<string> args,
        out LauncherStartOptions? options,
        out string errorMessage
    )
    {
        options = null;
        errorMessage = string.Empty;

        if (args.Count == 0)
        {
            errorMessage = "Missing launcher arguments.";
            return false;
        }

        string? targetPath = null;
        var targetArgs = new List<string>();
        var pipeName = $"asvl-{Guid.NewGuid():N}"[..13];
        var sessionToken = Guid.NewGuid().ToString("N");
        var startupTimeout = TimeSpan.FromSeconds(10);
        var passthroughMode = false;

        for (var i = 0; i < args.Count; i++)
        {
            var current = args[i];

            if (passthroughMode)
            {
                targetArgs.Add(current);
                continue;
            }

            switch (current)
            {
                case LauncherCommandLineArguments.PassthroughArgsSeparator:
                    passthroughMode = true;
                    break;
                case LauncherCommandLineArguments.TargetArg:
                    if (!TryReadValue(args, ref i, out targetPath, out errorMessage))
                    {
                        return false;
                    }
                    break;
                case LauncherCommandLineArguments.PipeArg:
                    if (!TryReadValue(args, ref i, out pipeName, out errorMessage))
                    {
                        return false;
                    }
                    break;
                case LauncherCommandLineArguments.TokenArg:
                    if (!TryReadValue(args, ref i, out sessionToken, out errorMessage))
                    {
                        return false;
                    }
                    break;
                case LauncherCommandLineArguments.TimeoutSecArg:
                    if (!TryReadValue(args, ref i, out var timeoutRaw, out errorMessage))
                    {
                        return false;
                    }

                    if (int.TryParse(timeoutRaw, out var timeoutSec) == false || timeoutSec <= 0)
                    {
                        errorMessage =
                            $"Invalid {LauncherCommandLineArguments.TimeoutSecArg} value: '{timeoutRaw}'.";
                        return false;
                    }

                    startupTimeout = TimeSpan.FromSeconds(timeoutSec);
                    break;
                default:
                    errorMessage = $"Unknown launcher argument: '{current}'.";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            errorMessage =
                $"Missing required argument: {LauncherCommandLineArguments.TargetArg} <path-to-executable>.";
            return false;
        }

        options = new LauncherStartOptions
        {
            TargetPath = targetPath,
            TargetArgs = targetArgs,
            PipeName = pipeName,
            SessionToken = sessionToken,
            StartupTimeout = startupTimeout,
        };
        return true;
    }

    private static bool TryReadValue(
        IReadOnlyList<string> args,
        ref int index,
        out string value,
        out string error
    )
    {
        var valueIndex = index + 1;
        if (valueIndex >= args.Count)
        {
            value = string.Empty;
            error = $"Missing value for argument: '{args[index]}'.";
            return false;
        }

        value = args[valueIndex];
        index = valueIndex;
        error = string.Empty;
        return true;
    }
}
