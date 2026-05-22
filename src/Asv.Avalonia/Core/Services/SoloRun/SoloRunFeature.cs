using System.IO.Pipes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

public class SoloRunFeature : BackgroundService
{
    private readonly IAppArgsStore _argsStore;
    private readonly ILogger<SoloRunFeature> _logger;
    private readonly string? _pipeName;
    private readonly Mutex _mutex;
    private readonly Lock _pipeServerSync = new();

    private NamedPipeServerStream? _pipeServer;
    private readonly bool _isFirstInstance;

    private volatile int _isMutexDisposed;
    private volatile int _isStopped;

    public SoloRunFeature(
        IOptions<SoloRunFeatureOptions> option,
        IAppArgsStore argsStore,
        ILoggerFactory loggerFactory
    )
    {
        _argsStore = argsStore;
        _logger = loggerFactory.CreateLogger<SoloRunFeature>();
        var config = option.Value;
        if (string.IsNullOrWhiteSpace(config.Mutex))
        {
            throw new InvalidOperationException("Mutex name is not set");
        }

        var mutexName = config.Mutex;
        _mutex = new Mutex(
            initiallyOwned: true,
            name: mutexName,
            options: new NamedWaitHandleOptions
            {
                CurrentUserOnly = true,
                CurrentSessionOnly = false,
            },
            createdNew: out var isNewInstance
        );
        _logger.LogTrace(
            "SoloRun: pid={Pid}, mutex='{Mutex}', isNew={IsNew}, user={User}, tmp={Tmp}, baseDir={BaseDir}",
            Environment.ProcessId,
            mutexName,
            isNewInstance,
            Environment.UserName,
            Environment.GetEnvironmentVariable("TMPDIR"),
            AppContext.BaseDirectory
        );

        _isFirstInstance = isNewInstance;
        if (!option.Value.ArgForward)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(option.Value.Pipe))
        {
            throw new InvalidOperationException("Pipe name is not set");
        }

        _pipeName = option.Value.Pipe;
        if (_isFirstInstance)
        {
            return;
        }

        var args = new AppArgs(Environment.GetCommandLineArgs());
        SendArgumentsToRunningInstance(args, option.Value.Pipe);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_pipeName) || !_isFirstInstance)
        {
            return;
        }

        await RunNamedPipeServerAsync(_pipeName, stoppingToken).ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _isStopped, 1, 0) != 0)
        {
            return;
        }

        DisposePipeServer();
        try
        {
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            DisposeMutex();
        }
    }

    public override void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isStopped, 1, 0) == 0)
        {
            DisposePipeServer();
            DisposeMutex();
        }

        base.Dispose();
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
        while (!cancellationToken.IsCancellationRequested)
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
                _argsStore.Set(AppArgs.DeserializeFromString(args));
            }
            catch (OperationCanceledException)
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
}
