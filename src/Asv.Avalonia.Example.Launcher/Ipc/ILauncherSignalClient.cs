using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Launcher.Contracts;

namespace Asv.Avalonia.Example.Launcher.Ipc;

public interface ILauncherSignalClient
{
    Task SendAsync(
        string pipeName,
        string sessionToken,
        LauncherSignal signal,
        CancellationToken cancellationToken = default
    );
}
