using System.Diagnostics;
using Asv.Avalonia.Launcher.Contracts;

namespace Asv.Avalonia.Launcher.Orchestration;

public sealed class TargetProcessRunner
{
    private static readonly TimeSpan ProcessTerminationWaitTimeout = TimeSpan.FromSeconds(2);

    public TargetProcessStartResult Start(ProcessStartInfo startInfo)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        var process = new Process { StartInfo = startInfo };

        try
        {
            if (!process.Start())
            {
                process.Dispose();
                return TargetProcessStartResult.Failed("Failed to start target process.");
            }

            return TargetProcessStartResult.Started(process);
        }
        catch (Exception ex)
        {
            process.Dispose();
            return TargetProcessStartResult.Failed($"Failed to start target process: {ex.Message}");
        }
    }

    public async Task TerminateAsync(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        var killRequested = false;
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                killRequested = true;
            }
        }
        catch
        {
            return;
        }

        if (!killRequested)
        {
            return;
        }

        try
        {
            using var timeoutCts = new CancellationTokenSource(ProcessTerminationWaitTimeout);
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch
        {
            // Best-effort cleanup after startup timeout.
        }
    }
}
