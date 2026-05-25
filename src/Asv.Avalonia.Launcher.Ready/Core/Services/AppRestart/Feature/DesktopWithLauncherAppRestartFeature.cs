using System.Diagnostics;
using Asv.Avalonia.Launcher.Api;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Launcher.Ready;

public sealed class DesktopWithLauncherAppRestartFeature(
    IAppArgsStore argsStore,
    ILoggerFactory loggerFactory
) : IAppRestartFeature
{
    private readonly ILogger<DesktopWithLauncherAppRestartFeature> _logger =
        loggerFactory.CreateLogger<DesktopWithLauncherAppRestartFeature>();

    public void Restart()
    {
        var appPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(appPath))
        {
            _logger.LogError("Failed to restart the application: process path is empty.");
            return;
        }

        var currentArgs = argsStore.Args.CurrentValue;
        if (!TryGetLauncherPath(currentArgs.Args, out var launcherPath))
        {
            _logger.LogError("Failed to restart the application: launcher path is missing.");
            return;
        }

        var targetArgs = currentArgs.RawArgs.Skip(1).Where(IsTargetArgument).ToArray();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = launcherPath,
                UseShellExecute = false,
                WorkingDirectory =
                    Path.GetDirectoryName(launcherPath) ?? Environment.CurrentDirectory,
            };
            psi.ArgumentList.Add(LauncherCommandLineArguments.TargetArg);
            psi.ArgumentList.Add(appPath);

            if (targetArgs.Length > 0)
            {
                psi.ArgumentList.Add(LauncherCommandLineArguments.PassthroughArgsSeparator);
                foreach (var arg in targetArgs)
                {
                    psi.ArgumentList.Add(arg);
                }
            }

            Process.Start(psi);
            _logger.LogInformation(
                "Application restarted through launcher successfully. Launcher path: {LauncherPath}. Target path: {AppPath}. Args: {Args}",
                launcherPath,
                appPath,
                string.Join(" ", targetArgs)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to restart the application through launcher. Launcher path: {LauncherPath}. Target path: {AppPath}. Args: {Args}",
                launcherPath,
                appPath,
                string.Join(" ", targetArgs)
            );
        }
    }

    private static bool TryGetLauncherPath(
        IReadOnlyDictionary<string, string> args,
        out string launcherPath
    )
    {
        return TryGetArgValue(args, LauncherCommandLineArguments.LauncherPathArg, out launcherPath);
    }

    private static bool TryGetArgValue(
        IReadOnlyDictionary<string, string> args,
        string key,
        out string value
    )
    {
        value = string.Empty;

        if (args.TryGetValue(key, out var rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        if (!key.StartsWith("--", StringComparison.Ordinal))
        {
            return false;
        }

        var normalizedKey = key[2..];
        if (args.TryGetValue(normalizedKey, out rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        return false;
    }

    private static bool IsTargetArgument(string arg)
    {
        return !IsLauncherArgument(arg, LauncherCommandLineArguments.LauncherPipeArg)
            && !IsLauncherArgument(arg, LauncherCommandLineArguments.LauncherTokenArg)
            && !IsLauncherArgument(arg, LauncherCommandLineArguments.LauncherPathArg);
    }

    private static bool IsLauncherArgument(string arg, string key)
    {
        if (string.Equals(arg, key, StringComparison.Ordinal))
        {
            return true;
        }

        if (arg.StartsWith($"{key}=", StringComparison.Ordinal))
        {
            return true;
        }

        if (!key.StartsWith("--", StringComparison.Ordinal))
        {
            return false;
        }

        var normalizedKey = key[2..];
        return string.Equals(arg, normalizedKey, StringComparison.Ordinal)
            || arg.StartsWith($"{normalizedKey}=", StringComparison.Ordinal);
    }
}
