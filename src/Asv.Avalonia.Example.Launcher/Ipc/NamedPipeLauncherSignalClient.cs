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
    private readonly TimeProvider _timeProvider;

    public NamedPipeLauncherSignalClient(
        TimeSpan? connectTimeout = null,
        TimeProvider? timeProvider = null
    )
    {
        _connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(5);
        _timeProvider = timeProvider ?? TimeProvider.System;
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

        var message = new LauncherIpcMessage
        {
            SessionToken = sessionToken,
            TimestampUtc = _timeProvider.GetUtcNow(),
            Signal = signal,
        };
        var payload = LauncherIpcJson.Serialize(message);

        using var pipe = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.Out,
            PipeOptions.Asynchronous
        );

        using var timeoutCts = new CancellationTokenSource(_connectTimeout, _timeProvider);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token
        );

        await pipe.ConnectAsync(linkedCts.Token).ConfigureAwait(false);

        await using var writer = new StreamWriter(pipe, Encoding.UTF8, leaveOpen: false);
        await writer.WriteAsync(payload.AsMemory(), linkedCts.Token).ConfigureAwait(false);
        await writer.FlushAsync(linkedCts.Token).ConfigureAwait(false);
    }
}
