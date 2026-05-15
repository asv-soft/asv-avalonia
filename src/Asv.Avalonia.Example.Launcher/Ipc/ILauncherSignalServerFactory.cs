namespace Asv.Avalonia.Example.Launcher.Ipc;

public interface ILauncherSignalServerFactory
{
    ILauncherSignalServer Create(string pipeName, string sessionToken);
}
