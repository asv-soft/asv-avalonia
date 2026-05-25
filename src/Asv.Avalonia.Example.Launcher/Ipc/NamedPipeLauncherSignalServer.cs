using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Example.Launcher.Ipc;

public sealed class NamedPipeLauncherSignalServer : ILauncherSignalServer
{
    private readonly string _pipeName;
    private readonly string _sessionToken;

    public NamedPipeLauncherSignalServer(string pipeName, string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(pipeName);
        ArgumentNullException.ThrowIfNull(sessionToken);

        _pipeName = pipeName;
        _sessionToken = sessionToken;
    }

    public async Task<LauncherIpcMessage> WaitForSignalAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using var pipe = new NamedPipeServerStream(
            _pipeName,
            PipeDirection.In,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous
        );

        await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

        using var reader = new StreamReader(
            pipe,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false
        );
        var payload = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        var message = LauncherIpcJson.Deserialize(payload);

        ValidateMessage(message);
        return message;
    }

    private void ValidateMessage(LauncherIpcMessage message)
    {
        if (
            !string.Equals(
                message.ProtocolVersion,
                LauncherIpcConstants.ProtocolVersion,
                StringComparison.Ordinal
            )
        )
        {
            throw new InvalidOperationException(
                $"Unsupported IPC protocol version '{message.ProtocolVersion}'."
            );
        }

        if (!string.Equals(message.SessionToken, _sessionToken, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("IPC session token mismatch.");
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
