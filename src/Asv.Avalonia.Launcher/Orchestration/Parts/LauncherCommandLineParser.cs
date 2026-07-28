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
        return TryParseCore(args, null, out options, out errorMessage, out _, out _);
    }

    /// <summary>
    /// Adds a default target executable when the launcher arguments are otherwise valid and do not
    /// specify one. Relative target paths are resolved against
    /// <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    /// <param name="args">The launcher command-line arguments.</param>
    /// <param name="defaultTargetPath">The default target executable path.</param>
    /// <returns>
    /// The original arguments when they specify a target or are invalid; otherwise, a new argument
    /// array containing the default target.
    /// </returns>
    public static string[] WithDefaultTarget(string[] args, string defaultTargetPath)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultTargetPath);

        var resolvedTargetPath = Path.GetFullPath(defaultTargetPath, AppContext.BaseDirectory);
        if (
            !TryParseCore(
                args,
                resolvedTargetPath,
                out _,
                out _,
                out var insertionIndex,
                out var usedDefaultTarget
            ) || !usedDefaultTarget
        )
        {
            return args;
        }

        var result = new string[args.Length + 2];
        Array.Copy(args, 0, result, 0, insertionIndex);
        result[insertionIndex] = LauncherCommandLineArguments.TargetArg;
        result[insertionIndex + 1] = resolvedTargetPath;
        Array.Copy(args, insertionIndex, result, insertionIndex + 2, args.Length - insertionIndex);

        return result;
    }

    private static bool TryParseCore(
        IReadOnlyList<string> args,
        string? defaultTargetPath,
        out LauncherStartOptions? options,
        out string errorMessage,
        out int targetInsertionIndex,
        out bool usedDefaultTarget
    )
    {
        options = null;
        errorMessage = string.Empty;
        targetInsertionIndex = args.Count;
        usedDefaultTarget = false;

        if (args.Count == 0 && string.IsNullOrWhiteSpace(defaultTargetPath))
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
        var targetArgumentSpecified = false;

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
                    targetInsertionIndex = i;
                    break;
                case LauncherCommandLineArguments.TargetArg:
                    targetArgumentSpecified = true;
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

        if (
            !targetArgumentSpecified
            && string.IsNullOrWhiteSpace(targetPath)
            && !string.IsNullOrWhiteSpace(defaultTargetPath)
        )
        {
            targetPath = defaultTargetPath;
            usedDefaultTarget = true;
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
