namespace Asv.Avalonia;

public interface IPluginManuallyInstallable
{
    Task InstallManually(
        string from,
        IPluginServerInfo source,
        string packageId,
        string version,
        IProgress<ProgressMessage>? progress,
        CancellationToken cancel
    );
}
