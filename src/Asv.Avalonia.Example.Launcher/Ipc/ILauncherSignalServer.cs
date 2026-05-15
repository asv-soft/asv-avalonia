using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Launcher.Contracts;

namespace Asv.Avalonia.Example.Launcher.Ipc;

public interface ILauncherSignalServer : IAsyncDisposable
{
    Task<LauncherIpcMessage> WaitForSignalAsync(CancellationToken cancellationToken = default);
}
