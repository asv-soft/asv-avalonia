using System.IO.Pipes;
using Asv.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class SoloRunFeatureConfig
{
    public const string Section = "SoloRun";
    public string? Mutex { get; set; }
    public bool ArgForward { get; set; }
    public string? Pipe { get; set; }
}

public class SoloRunFeature : AsyncDisposableWithCancel, ISoloRunFeature
{
    private readonly Mutex _mutex;
    private readonly ReactiveProperty<AppArgs> _args;

    public SoloRunFeature(IOptions<SoloRunFeatureConfig> option, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<SoloRunFeature>();
        _args = new ReactiveProperty<AppArgs>(new AppArgs(Environment.GetCommandLineArgs()));
        var config = option.Value;
        _mutex = new Mutex(true, config.Mutex, out var isNewInstance);
        IsFirstInstance = isNewInstance;
        if (option.Value.ArgForward)
        {
            if (string.IsNullOrWhiteSpace(option.Value.Pipe))
            {
                throw new InvalidOperationException("Pipe name is not set");
            }

            if (IsFirstInstance)
            {
                StartNamedPipeServer(option.Value.Pipe, logger);
            }
            else
            {
                var args = new AppArgs(Environment.GetCommandLineArgs());
                SendArgumentsToRunningInstance(args, option.Value.Pipe, logger);
            }
        }
    }

    public bool IsFirstInstance { get; }

    public ReadOnlyReactiveProperty<AppArgs> Args => _args;

    private void StartNamedPipeServer(string pipeName, ILogger logger)
    {
        new Thread(start: () =>
        {
            while (IsDisposed == false)
            {
                try
                {
                    using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
                    server.WaitForConnection();

                    using var reader = new StreamReader(server);
                    var args = reader.ReadLine();

                    if (args != null)
                    {
                        logger.ZLogInformation(
                            $"Received arguments from the named pipe {pipeName}."
                        );
                        _args.OnNext(_args.Value);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Named pipe server error.");
                }
            }

            // ReSharper disable once FunctionNeverReturns
        })
        {
            IsBackground = true,
        }.Start();
    }

    private static void SendArgumentsToRunningInstance(
        AppArgs args,
        string pipeName,
        ILogger logger
    )
    {
        try
        {
            logger.ZLogInformation(
                $"Sending arguments to the running instance through the named pipe {pipeName}."
            );
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(1000);
            using var writer = new StreamWriter(client);
            writer.WriteLine(args.SerializeToString());
            writer.Flush();
        }
        catch (Exception ex)
        {
            logger.ZLogError(
                ex,
                $"Failed to send arguments to the running instance through the named pipe {pipeName}."
            );
        }
    }
}
