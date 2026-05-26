namespace Asv.Avalonia.Launcher.Ipc;

public interface ILauncherSignalServerFactory
{
    ILauncherSignalServer Create(string pipeName, string sessionToken);
}
