using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Launcher.Ipc;

public interface ILauncherSignalServer : IAsyncDisposable
{
    Task<LauncherIpcMessage> WaitForSignalAsync(CancellationToken cancellationToken = default);
}
