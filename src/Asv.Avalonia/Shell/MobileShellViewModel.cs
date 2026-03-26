using System.Diagnostics;
using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class MobileShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.mobile";

    public MobileShellViewModel(
        IServiceProvider containerHost,
        IConfiguration cfg,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(ShellId, containerHost, loggerFactory, cfg, ext)
    {
        // do nothing
    }

    protected override void RestartApplication(string[] args)
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
        {
            Logger.LogError("Failed to get path of the application");
            return;
        }

        StartProcess(exePath, args);
    }

    private void StartProcess(string exePath, string[] args)
    {
        var psi = new ProcessStartInfo { FileName = exePath, UseShellExecute = false };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        Process.Start(psi);
        Logger.LogInformation(
            "Application restarted successfully with arguments: {Args} and path {ExePath}.",
            string.Join(" ", args),
            exePath
        );
    }
}
