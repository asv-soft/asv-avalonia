using System.IO.Pipes;
using Asv.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3;
using ZLogger;
using Lock = System.Threading.Lock;

namespace Asv.Avalonia;

public class SoloRunFeatureOptions
{
    public const string Section = "SoloRun";
    public string? Mutex { get; set; }
    public bool ArgForward { get; set; }
    public string? Pipe { get; set; }
}

public class SoloRunFeature : AsyncDisposableWithCancel, ISoloRunFeature
{
    private readonly SynchronizedReactiveProperty<IAppArgs> _args;
    private readonly ILogger<SoloRunFeature> _logger;
    private readonly string? _pipeName;
    private readonly Mutex _mutex;
    private readonly Lock _pipeServerSync = new();

    private Task? _pipeServerTask;
    private CancellationTokenSource? _pipeServerCts;
    private NamedPipeServerStream? _pipeServer;

    private volatile int _isMutexDisposed;

    public SoloRunFeature(IOptions<SoloRunFeatureOptions> option, ILoggerFactory loggerFactory)
    {
        _args = new SynchronizedReactiveProperty<IAppArgs>(
            new AppArgs(Environment.GetCommandLineArgs())
        );
        _logger = loggerFactory.CreateLogger<SoloRunFeature>();
        var config = option.Value;
        if (string.IsNullOrWhiteSpace(config.Mutex))
        {
            throw new InvalidOperationException("Mutex name is not set");
        }

        _mutex = new Mutex(true, config.Mutex, out var isNewInstance);
        IsFirstInstance = isNewInstance;
        if (!option.Value.ArgForward)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(option.Value.Pipe))
        {
            throw new InvalidOperationException("Pipe name is not set");
        }

        _pipeName = option.Value.Pipe;
        if (IsFirstInstance)
        {
            return;
        }

        var args = new AppArgs(Environment.GetCommandLineArgs());
        SendArgumentsToRunningInstance(args, option.Value.Pipe);
    }

    public bool IsFirstInstance { get; }
    public ReadOnlyReactiveProperty<IAppArgs> Args => _args;

    public Task StartAsync(CancellationToken cancel = default)
    {
        if (string.IsNullOrWhiteSpace(_pipeName) || !IsFirstInstance || _pipeServerTask is not null)
        {
            return Task.CompletedTask;
        }

        cancel.ThrowIfCancellationRequested();
        _pipeServerCts = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        _pipeServerTask = Task.Factory.StartNew(
            async _ => await RunNamedPipeServerAsync(_pipeName, _pipeServerCts.Token),
            TaskCreationOptions.LongRunning,
            _pipeServerCts.Token
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancel = default)
    {
        if (IsDisposed)
        {
            return Task.CompletedTask;
        }

        if (cancel.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var cts = Interlocked.Exchange(ref _pipeServerCts, null);
        cts?.Cancel();

        DisposePipeServer();

        var pipeServerTask = Interlocked.Exchange(ref _pipeServerTask, null);
        pipeServerTask?.Dispose();

        cts?.Dispose();
        DisposeMutex();
        return Task.CompletedTask;
    }

    private void DisposeMutex()
    {
        if (Interlocked.CompareExchange(ref _isMutexDisposed, 1, 0) != 0)
        {
            return;
        }

        try
        {
            _mutex.ReleaseMutex();
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "The current thread does not own the mutex.");
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Mutex already disposed.");
            return;
        }

        _mutex.Dispose();
    }

    private async Task RunNamedPipeServerAsync(string pipeName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (!cancellationToken.IsCancellationRequested && !IsDisposed)
        {
            try
            {
                await using var pipeServer = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous
                );

                using (_pipeServerSync.EnterScope())
                {
                    _pipeServer = pipeServer;
                }

                await pipeServer.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                using var reader = new StreamReader(pipeServer);
                var args = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(args))
                {
                    continue;
                }

                _logger.ZLogInformation($"Received arguments from the named pipe {pipeName}.");
                _args.OnNext(AppArgs.DeserializeFromString(args));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Named pipe server error.");
            }
            finally
            {
                DisposePipeServer();
            }
        }
    }

    private void DisposePipeServer()
    {
        using (_pipeServerSync.EnterScope())
        {
            if (_pipeServer?.IsConnected == true)
            {
                _pipeServer.Disconnect();
            }

            _pipeServer?.Dispose();
            _pipeServer = null;
        }
    }

    private void SendArgumentsToRunningInstance(AppArgs args, string pipeName)
    {
        try
        {
            _logger.ZLogInformation(
                $"Sending arguments to the running instance through the named pipe {pipeName}."
            );
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(1000);
            using var writer = new StreamWriter(client);
            writer.Write(args.SerializeToString());
            writer.Flush();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(
                ex,
                $"Failed to send arguments to the running instance through the named pipe {pipeName}."
            );
        }
        finally
        {
            _logger.LogInformation(
                "Shutting down the current instance because it is the second instance."
            );
            Environment.Exit(0);
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await StopAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopAsync().GetAwaiter().GetResult();
        }
    }
}
