﻿using System.Diagnostics;
using System.IO.Pipes;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class AppHostConfig
{
    public int RollingSizeKb { get; set; } = 1024 * 10;
    public LogLevel LogMinLevel { get; set; } = LogLevel.Information;
}

public sealed class AppHost : IAppHost
{
    #region Static

    private static IAppHost _instance = null!;
    public static IAppHost Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException(
                    $"{nameof(AppHost)} not initialized. Please call {nameof(Initialize)} method first."
                );
            return _instance;
        }
    }

    public static IAppHost Initialize(Action<IAppHostBuilder> configure)
    {
        if (_instance != null)
            throw new InvalidOperationException($"{nameof(AppHost)} already initialized.");
        var builder = new AppHostBuilder();
        configure(builder);
        _instance = builder.Create();
        return _instance;
    }

    #endregion

    private readonly Mutex? _mutex;
    private readonly ReactiveProperty<AppArgs> _args = new();
    private readonly Subject<Exception> _errorHandler = new();

    internal AppHost(
        IConfiguration config,
        AppPath appPath,
        AppInfo appInfo,
        ILoggerFactory logFactory,
        AppArgs args,
        string? mutexName,
        string? argsPipeName
    )
    {
        Configuration = config;
        AppPath = appPath;
        AppInfo = appInfo;
        LoggerFactory = logFactory;
        var logger = logFactory.CreateLogger($"{nameof(AppHost)}[PID:{Environment.ProcessId}]");
        SetupExceptionHandlers(logger);
        if (mutexName != null)
        {
            _mutex = new Mutex(true, mutexName, out var isNewInstance);
            IsFirstInstance = isNewInstance;
        }
        if (argsPipeName != null)
        {
            if (_mutex == null)
            {
                logger.LogError("Named pipe can be used only with single instance mode.");
                throw new InvalidOperationException(
                    "Named pipe can be used only with single instance mode."
                );
            }
            if (IsFirstInstance)
            {
                StartNamedPipeServer(argsPipeName, logger);
            }
            else
            {
                SendArgumentsToRunningInstance(args, argsPipeName, logger);
            }
        }
    }

    public ReadOnlyReactiveProperty<AppArgs> Args => _args;
    public IAppInfo AppInfo { get; }
    public IAppPath AppPath { get; }
    public IConfiguration Configuration { get; }
    public ILoggerFactory LoggerFactory { get; }

    private void SetupExceptionHandlers(ILogger logger)
    {
        _errorHandler.Subscribe(ex =>
        {
            logger.ZLogCritical(ex, $"Unhandled exception: {ex.Message}");
            Debug.Fail($"Unhandled exception: {ex.Message}");
        });
        ObservableSystem.RegisterUnhandledExceptionHandler(ex =>
        {
            logger.ZLogCritical(ex, $"R3 unobserved exception: {ex.Message}");
            Debug.Fail($"R3 unobserved exception: {ex.Message}");
        });
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            logger.ZLogCritical(
                args.Exception,
                $"Task scheduler unobserved task exception from '{sender}': {args.Exception.Message}"
            );
            Debug.Fail($"R3 unobserved exception: {args.Exception.Message}");
            args.SetObserved();
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
        {
            logger.ZLogCritical(
                $"Unhandled AppDomain exception. Sender '{sender}'. Args: {eventArgs.ExceptionObject}"
            );
            Debug.Fail($"R3 unobserved exception: {eventArgs.ExceptionObject}");
        };
    }

    public bool AllowOnlyOneInstance => _mutex != null;
    public bool IsFirstInstance { get; } = true;

    private void StartNamedPipeServer(string pipeName, ILogger logger)
    {
        new Thread(start: () =>
        {
            while (true)
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
                        var appArgs = AppArgs.DeserializeFromString(args);
                        _args.OnNext(appArgs);
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

    private void SendArgumentsToRunningInstance(AppArgs args, string pipeName, ILogger logger)
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

    #region Handle exceptions

    public void HandleApplicationCrash(Exception exception)
    {
        _errorHandler.OnNext(exception);
    }

    #endregion

    public void Dispose()
    {
        _mutex?.Dispose();
        _args.Dispose();
        _errorHandler.Dispose();
        Configuration.Dispose();
        LoggerFactory.Dispose();
    }
}
