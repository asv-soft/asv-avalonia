using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class DesktopAppRestartFeature(IAppArgsStore argsStore, ILoggerFactory loggerFactory)
    : IAppRestartFeature
{
    private readonly ILogger<DesktopAppRestartFeature> _logger =
        loggerFactory.CreateLogger<DesktopAppRestartFeature>();

    public void Restart()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
        {
            _logger.LogError("Failed to restart the application: process path is empty.");
            return;
        }

        var args = argsStore.Args.CurrentValue.RawArgs.Skip(1).ToArray();

        try
        {
            var psi = new ProcessStartInfo { FileName = exePath, UseShellExecute = false };
            foreach (var arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            Process.Start(psi);
            _logger.LogInformation(
                "Application restarted successfully. Path: {ExePath}. Args: {Args}",
                exePath,
                string.Join(" ", args)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to restart the application. Path: {ExePath}. Args: {Args}",
                exePath,
                string.Join(" ", args)
            );
        }
    }
}
