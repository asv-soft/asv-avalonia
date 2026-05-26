using System.Diagnostics;
using Asv.Avalonia.Example.Launcher.Contracts;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Example.Launcher.Orchestration;

public sealed class ProcessStartInfoFactory
{
    private readonly Func<string?> _getLauncherPath;

    public ProcessStartInfoFactory(Func<string?> getLauncherPath)
    {
        ArgumentNullException.ThrowIfNull(getLauncherPath);
        _getLauncherPath = getLauncherPath;
    }

    public ProcessStartInfo Create(LauncherStartOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var startInfo = new ProcessStartInfo
        {
            FileName = options.TargetPath,
            WorkingDirectory =
                Path.GetDirectoryName(options.TargetPath) ?? Environment.CurrentDirectory,
        };

        foreach (var targetArg in options.TargetArgs)
        {
            startInfo.ArgumentList.Add(targetArg);
        }

        startInfo.ArgumentList.Add(
            $"{LauncherCommandLineArguments.LauncherPipeArg}={options.PipeName}"
        );
        startInfo.ArgumentList.Add(
            $"{LauncherCommandLineArguments.LauncherTokenArg}={options.SessionToken}"
        );

        var launcherPath = _getLauncherPath();
        if (!string.IsNullOrWhiteSpace(launcherPath))
        {
            startInfo.ArgumentList.Add(
                $"{LauncherCommandLineArguments.LauncherPathArg}={launcherPath}"
            );
        }

        return startInfo;
    }
}
