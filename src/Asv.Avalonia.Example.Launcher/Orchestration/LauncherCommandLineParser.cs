using System;
using System.Collections.Generic;
using Asv.Avalonia.Example.Launcher.Contracts;

namespace Asv.Avalonia.Example.Launcher.Orchestration;

public static class LauncherCommandLineParser
{
    public static bool TryParse(string[] args, out LauncherStartOptions? options, out string error)
    {
        options = null;
        error = string.Empty;

        if (args.Length == 0)
        {
            error = "Missing launcher arguments.";
            return false;
        }

        string? targetPath = null;
        var targetArgs = new List<string>();
        var pipeName = $"asv-launcher-{Guid.NewGuid():N}";
        var sessionToken = Guid.NewGuid().ToString("N");
        var startupTimeout = TimeSpan.FromSeconds(60);
        var passthroughMode = false;

        for (var i = 0; i < args.Length; i++)
        {
            var current = args[i];

            if (passthroughMode)
            {
                targetArgs.Add(current);
                continue;
            }

            switch (current)
            {
                case "--":
                    passthroughMode = true;
                    break;
                case "--target":
                    if (!TryReadValue(args, ref i, out targetPath, out error))
                    {
                        return false;
                    }
                    break;
                case "--pipe":
                    if (!TryReadValue(args, ref i, out pipeName, out error))
                    {
                        return false;
                    }
                    break;
                case "--token":
                    if (!TryReadValue(args, ref i, out sessionToken, out error))
                    {
                        return false;
                    }
                    break;
                case "--timeout-sec":
                    if (!TryReadValue(args, ref i, out var timeoutRaw, out error))
                    {
                        return false;
                    }

                    if (int.TryParse(timeoutRaw, out var timeoutSec) == false || timeoutSec <= 0)
                    {
                        error = $"Invalid --timeout-sec value: '{timeoutRaw}'.";
                        return false;
                    }

                    startupTimeout = TimeSpan.FromSeconds(timeoutSec);
                    break;
                default:
                    error = $"Unknown launcher argument: '{current}'.";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            error = "Missing required argument: --target <path-to-executable>.";
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
        string[] args,
        ref int index,
        out string value,
        out string error
    )
    {
        var valueIndex = index + 1;
        if (valueIndex >= args.Length)
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
