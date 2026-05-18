using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Example.Launcher.Ipc;

public sealed class NamedPipeLauncherSignalClient : ILauncherSignalClient
{
    private readonly TimeSpan _connectTimeout;

    public NamedPipeLauncherSignalClient(TimeSpan? connectTimeout = null)
    {
        _connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(5);
    }

    public async Task SendAsync(
        string pipeName,
        string sessionToken,
        LauncherSignal signal,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new ArgumentException("Pipe name is required.", nameof(pipeName));
        }

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            throw new ArgumentException("Session token is required.", nameof(sessionToken));
        }

        ArgumentNullException.ThrowIfNull(signal);

        var message = new LauncherIpcMessage { SessionToken = sessionToken, Signal = signal };
        var payload = LauncherIpcJson.Serialize(message);

        using var pipe = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.Out,
            PipeOptions.Asynchronous
        );

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_connectTimeout);

        await pipe.ConnectAsync(timeoutCts.Token).ConfigureAwait(false);

        await using var writer = new StreamWriter(pipe, Encoding.UTF8, leaveOpen: false);
        await writer.WriteAsync(payload.AsMemory(), timeoutCts.Token).ConfigureAwait(false);
        await writer.FlushAsync(timeoutCts.Token).ConfigureAwait(false);
    }
}
