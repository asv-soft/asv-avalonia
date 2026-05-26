namespace Asv.Avalonia.Launcher.Ipc;

public sealed class NamedPipeLauncherSignalServerFactory : ILauncherSignalServerFactory
{
    public ILauncherSignalServer Create(string pipeName, string sessionToken)
    {
        return new NamedPipeLauncherSignalServer(pipeName, sessionToken);
    }
}
