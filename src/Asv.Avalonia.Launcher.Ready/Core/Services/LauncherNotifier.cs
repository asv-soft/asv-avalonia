using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Asv.Avalonia.Launcher.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Launcher.Ready;

#pragma warning disable SA1313
internal readonly record struct LauncherReadyEndpoint(string PipeName, string SessionToken);
#pragma warning restore SA1313

internal sealed class LauncherNotifier
{
    public const string LauncherPipeArg = LauncherCommandLineArguments.LauncherPipeArg;
    public const string LauncherTokenArg = LauncherCommandLineArguments.LauncherTokenArg;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(3);

    private readonly TimeProvider _timeProvider;
    private readonly LauncherReadyEndpoint? _endpoint;
    private readonly ILogger<LauncherNotifier> _logger;

    public LauncherNotifier(
        IAppArgsStore appArgsStore,
        TimeProvider timeProvider,
        IOptions<LauncherFeatureOptions> options,
        ILogger<LauncherNotifier> logger
    )
    {
        ArgumentNullException.ThrowIfNull(appArgsStore);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _timeProvider = timeProvider;
        _logger = logger;

        _endpoint = TryGetEndpointFromArgs(appArgsStore.Args.CurrentValue.RawArgs);
        if (_endpoint.HasValue || options.Value.IsOptional)
        {
            return;
        }

        throw new InvalidOperationException("Failed to read launcher endpoint.");
    }

    public async Task NotifyReadyAsync(CancellationToken cancellationToken = default)
    {
        if (_endpoint is null)
        {
            _logger.LogDebug(
                "Launcher endpoint arguments are missing. READY signal notification is skipped."
            );
            return;
        }

        var endpoint = _endpoint.Value;

        var message = new LauncherIpcMessage
        {
            SessionToken = endpoint.SessionToken,
            TimestampUtc = _timeProvider.GetUtcNow(),
            Signal = new LauncherSignal(LauncherSignalType.Ready, "Main UI ready.", 1.0),
        };

        var payload = JsonSerializer.Serialize(message, SerializerOptions);
        await using var pipe = new NamedPipeClientStream(
            ".",
            endpoint.PipeName,
            PipeDirection.Out,
            PipeOptions.Asynchronous
        );

        using var timeoutCts = new CancellationTokenSource(ConnectTimeout, _timeProvider);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token
        );

        await pipe.ConnectAsync(linkedCts.Token).ConfigureAwait(false);

        await using var writer = new StreamWriter(pipe, Encoding.UTF8, leaveOpen: false);
        await writer.WriteAsync(payload.AsMemory(), linkedCts.Token).ConfigureAwait(false);
        await writer.FlushAsync(linkedCts.Token).ConfigureAwait(false);
    }

    private static LauncherReadyEndpoint? TryGetEndpointFromArgs(in IReadOnlyList<string> args)
    {
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
            return null;
        }

        return new LauncherReadyEndpoint(pipeName, sessionToken);
    }
}
