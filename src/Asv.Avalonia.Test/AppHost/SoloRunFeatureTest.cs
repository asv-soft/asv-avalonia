using System.Collections.Concurrent;
using System.IO.Pipes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class SoloRunFeatureTest
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact(Skip = "Unix socket path limits do not apply on Windows.", SkipWhen = nameof(IsWindows))]
    public async Task StartAsync_OverlongUnixPipePath_LogsOnceAndStopsForwarding()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var logger = new ErrorLogger();
        logger.OnError = () =>
        {
            if (logger.Errors.Count >= 2)
            {
                cancellation.Cancel();
            }
        };
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(logger));
        await using var argsStore = new AppArgsStore();
        using var feature = CreateFeature(new string('p', 512), argsStore, loggerFactory);

        await feature.StartAsync(cancellation.Token);
        await feature.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5));
        await feature.StopAsync(TestContext.Current.CancellationToken);

        Assert.IsAssignableFrom<ArgumentException>(Assert.Single(logger.Errors));
    }

    [Fact]
    public async Task ExecuteAsync_MalformedPayload_ContinuesForwardingValidArguments()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var logger = new ErrorLogger();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(logger));
        await using var argsStore = new AppArgsStore();
        var received = new TaskCompletionSource<IAppArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = argsStore.Args.Subscribe(args =>
        {
            if (args.Args.ContainsKey("test"))
            {
                received.TrySetResult(args);
            }
        });
        var pipeName = Guid.NewGuid().ToString("N")[..12];
        using var feature = CreateFeature(pipeName, argsStore, loggerFactory);
        await feature.StartAsync(cancellation.Token);

        await SendAsync(pipeName, "{invalid-json", cancellation.Token);
        await logger.FirstError.Task.WaitAsync(cancellation.Token);
        await SendAsync(pipeName, new AppArgs(["--test=forwarded", "card.fasx"]).SerializeToString(), cancellation.Token);
        var actual = await received.Task.WaitAsync(cancellation.Token);
        await feature.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal("forwarded", actual.Args["test"]);
        Assert.Equal(["card.fasx"], actual.Tags);
        Assert.Equal(["--test=forwarded", "card.fasx"], actual.RawArgs);
        Assert.Single(logger.Errors);
    }

    [Fact]
    public async Task StopAsync_ConnectedClientWithoutPayload_DoesNotLogAnError()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var logger = new ErrorLogger();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(logger));
        await using var argsStore = new AppArgsStore();
        var pipeName = Guid.NewGuid().ToString("N")[..12];
        using var feature = CreateFeature(pipeName, argsStore, loggerFactory);
        await feature.StartAsync(cancellation.Token);
        await using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
        await client.ConnectAsync(cancellation.Token);

        await feature.StopAsync(cancellation.Token);

        Assert.Empty(logger.Errors);
        Assert.True(feature.ExecuteTask!.IsCompletedSuccessfully);
    }

    private static SoloRunFeature CreateFeature(string pipeName, IAppArgsStore argsStore, ILoggerFactory loggerFactory) =>
        new(
            Options.Create(new SoloRunFeatureOptions
            {
                Mutex = $"solo-test-{Guid.NewGuid():N}",
                ArgForward = true,
                Pipe = pipeName,
            }),
            argsStore,
            loggerFactory
        );

    private static async Task SendAsync(string pipeName, string payload, CancellationToken cancellationToken)
    {
        await using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
        await client.ConnectAsync(cancellationToken);
        await using var writer = new StreamWriter(client);
        await writer.WriteAsync(payload.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }

    private sealed class ErrorLogger : ILoggerProvider, ILogger
    {
        public ConcurrentQueue<Exception> Errors { get; } = new();
        public TaskCompletionSource FirstError { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Action? OnError { get; set; }

        public ILogger CreateLogger(string categoryName) => this;
        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public void Dispose() { }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel < LogLevel.Error || exception is null)
            {
                return;
            }

            Errors.Enqueue(exception);
            FirstError.TrySetResult();
            OnError?.Invoke();
        }
    }
}
