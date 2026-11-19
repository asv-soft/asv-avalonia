using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Launcher.Ready;

internal static class LauncherArgs
{
    public static bool TryGetLauncherPath(IAppArgs args, out string launcherPath)
    {
        return TryGetValue(args, LauncherCommandLineArguments.LauncherPathArg, out launcherPath);
    }

    public static bool TryGetValue(IAppArgs args, string key, out string value)
    {
        value = string.Empty;

        if (args.Args.TryGetValue(key, out var rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        if (!key.StartsWith("--", StringComparison.Ordinal))
        {
            return false;
        }

        var normalizedKey = key[2..];
        if (
            args.Args.TryGetValue(normalizedKey, out rawValue)
            && !string.IsNullOrWhiteSpace(rawValue)
        )
        {
            value = rawValue;
            return true;
        }

        return false;
    }
}
