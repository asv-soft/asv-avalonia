using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Asv.Avalonia.Launcher.Api;

namespace Asv.Avalonia.Launcher.Ready;

#pragma warning disable SA1313
internal readonly record struct LauncherReadyEndpoint(string PipeName, string SessionToken);
#pragma warning restore SA1313

internal static class LauncherReadyNotifier
{
    public const string LauncherPipeArg = LauncherCommandLineArguments.LauncherPipeArg;
    public const string LauncherTokenArg = LauncherCommandLineArguments.LauncherTokenArg;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public static bool TryReadEndpoint(
        IReadOnlyList<string> args,
        out LauncherReadyEndpoint endpoint
    )
    {
        ArgumentNullException.ThrowIfNull(args);

        endpoint = default;
        string? pipeName = null;
        string? sessionToken = null;

        for (var i = 0; i < args.Count; i++)
        {
            var current = args[i];

            if (string.Equals(current, LauncherPipeArg, StringComparison.Ordinal))
            {
                var valueIndex = i + 1;
                if (valueIndex < args.Count)
                {
                    pipeName = args[valueIndex];
                    i = valueIndex;
                }
                continue;
            }

            if (string.Equals(current, LauncherTokenArg, StringComparison.Ordinal))
            {
                var valueIndex = i + 1;
                if (valueIndex < args.Count)
                {
                    sessionToken = args[valueIndex];
                    i = valueIndex;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(pipeName) || string.IsNullOrWhiteSpace(sessionToken))
        {
            return false;
        }

        endpoint = new LauncherReadyEndpoint(pipeName, sessionToken);
        return true;
    }

    public static async Task NotifyReadyAsync(
        LauncherReadyEndpoint endpoint,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(endpoint.PipeName))
        {
            throw new ArgumentException("Pipe name is required.", nameof(endpoint));
        }

        if (string.IsNullOrWhiteSpace(endpoint.SessionToken))
        {
            throw new ArgumentException("Session token is required.", nameof(endpoint));
        }

        var message = new LauncherIpcMessage
        {
            SessionToken = endpoint.SessionToken,
            Signal = new LauncherSignal(LauncherSignalType.Ready, "Main UI ready.", 1.0),
        };

        var payload = JsonSerializer.Serialize(message, SerializerOptions);
        using var pipe = new NamedPipeClientStream(
            ".",
            endpoint.PipeName,
            PipeDirection.Out,
            PipeOptions.Asynchronous
        );

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

        await pipe.ConnectAsync(timeoutCts.Token).ConfigureAwait(false);

        await using var writer = new StreamWriter(pipe, Encoding.UTF8, leaveOpen: false);
        await writer.WriteAsync(payload.AsMemory(), timeoutCts.Token).ConfigureAwait(false);
        await writer.FlushAsync(timeoutCts.Token).ConfigureAwait(false);
    }
}
