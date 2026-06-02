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

internal sealed class LauncherNotifier : ILauncherNotifier
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

        var appArgs = appArgsStore.Args.CurrentValue;
        if (appArgs is null)
        {
            if (options.Value.IsOptional)
            {
                _logger.LogDebug(
                    "Launcher is optional, but application arguments are not available."
                );
                return;
            }

            throw new InvalidOperationException("Failed to read application arguments.");
        }

        foreach (var arg in appArgs.Args.Values)
        {
            _logger.LogDebug("Launcher args {arg}", arg);
        }
        foreach (var tag in appArgs.Tags)
        {
            _logger.LogDebug("Launcher tags {tag}", tag);
        }

        _endpoint = TryGetEndpointFromArgs(appArgs.Args);
        if (_endpoint.HasValue)
        {
            _logger.LogDebug("Launcher endpoint arguments are present.");
            return;
        }

        if (options.Value.IsOptional)
        {
            _logger.LogDebug(
                "Launcher is optional, but launcher's endpoint arguments were not presented"
            );
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

    private LauncherReadyEndpoint? TryGetEndpointFromArgs(
        in IReadOnlyDictionary<string, string> args
    )
    {
        var hasPipeName = TryGetArgValue(args, LauncherPipeArg, out var pipeName);
        var hasSessionToken = TryGetArgValue(args, LauncherTokenArg, out var sessionToken);

        if (!hasPipeName || !hasSessionToken)
        {
            return null;
        }

        _logger.LogDebug(
            "Launcher endpoint arguments are present: {PipeName} {SessionToken}",
            pipeName,
            sessionToken
        );
        return new LauncherReadyEndpoint(pipeName, sessionToken);
    }

    private static bool TryGetArgValue(
        in IReadOnlyDictionary<string, string> args,
        string key,
        out string value
    )
    {
        value = string.Empty;

        if (args.TryGetValue(key, out var rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        if (!key.StartsWith("--", StringComparison.Ordinal))
        {
            return false;
        }

        var normalizedKey = key[2..];
        if (args.TryGetValue(normalizedKey, out rawValue) && !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        return false;
    }
}
