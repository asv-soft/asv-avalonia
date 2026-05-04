using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLogger;

namespace Asv.Avalonia;

public static class UnhandledExceptions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseUnhandledExceptionsHandler()
        {
            builder
                .Services.AddSingleton<IUnhandledExceptionHandler, UnhandledExceptionsHandler>()
                .AddHostedService<UnhandledExceptionsHandler>()
                .AddOptions<UnhandledExceptionsHandlerOptions>()
                .BindConfiguration(UnhandledExceptionsHandlerOptions.ConfigurationSection)
                .ValidateOnStart();
            return builder;
        }
    }
}

public class UnhandledExceptionsHandlerOptions
{
    public const string ConfigurationSection = "UnhandledExceptions";
    public bool IsEnabled { get; set; } = true;
    public HandlerOptions R3 { get; } = new();
    public HandlerOptions TaskScheduler { get; } = new();
    public HandlerOptions AppDomain { get; } = new();
    public HandlerOptions UiThread { get; } = new();

    public class HandlerOptions
    {
        public bool PublishToShell { get; init; } = true;
        public bool PublishToLogger { get; init; } = true;
        public bool ForceApplicationCrash { get; init; } = false;
    }
}

public class UnhandledExceptionsHandler(
    IOptions<UnhandledExceptionsHandlerOptions> options,
    IShellHost shellHost,
    ILoggerFactory loggerFactory
) : IHostedService, IUnhandledExceptionHandler
{
    private readonly ILogger<UnhandledExceptionsHandler> _logger =
        loggerFactory.CreateLogger<UnhandledExceptionsHandler>();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (options.Value.IsEnabled == false)
        {
            _logger.LogInformation("Unhandled exception handler is disabled");
            return Task.CompletedTask;
        }

        _logger.ZLogInformation($"Unhandled exception handler is enabled: {options}");

        Dispatcher.UIThread.UnhandledException += OnUiUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += AppDomainException;
        R3.ObservableSystem.RegisterUnhandledExceptionHandler(R3UnhandledException);
        TaskScheduler.UnobservedTaskException += TaskSchedulerUnhandledException;
        return Task.CompletedTask;
    }

    private void OnUiUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (options.Value.UiThread.PublishToShell && shellHost.Shell is not null)
        {
            shellHost.Shell.ShowMessage(
                new ShellMessage("UI error", e.Exception.ToString(), ShellErrorState.Error)
            );
        }
        if (options.Value.UiThread.PublishToLogger)
        {
            _logger.ZLogError(e.Exception, $"Unhandled exception in AppDomain: {e.Exception.Message}");
        }
        if (options.Value.UiThread.ForceApplicationCrash)
        {
            Dispatcher.UIThread.Invoke(() => throw e.Exception);
        }
        e.Handled = true;
    }

    private void AppDomainException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        if (ex == null)
        {
            return;
        }
        if (options.Value.AppDomain.PublishToShell && shellHost.Shell is not null)
        {
            shellHost.Shell.ShowMessage(
                new ShellMessage("AppDomain error", ex.ToString(), ShellErrorState.Error)
            );
        }
        if (options.Value.AppDomain.PublishToLogger)
        {
            _logger.ZLogError(ex, $"Unhandled exception in AppDomain: {ex.Message}");
        }
        if (options.Value.AppDomain.ForceApplicationCrash)
        {
            Dispatcher.UIThread.Invoke(() => throw ex);
        }
    }

    private void TaskSchedulerUnhandledException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var eException = e.Exception;
        if (options.Value.TaskScheduler.PublishToShell && shellHost.Shell is not null)
        {
            shellHost.Shell.ShowMessage(
                new ShellMessage(
                    "Task scheduler error",
                    eException.ToString(),
                    ShellErrorState.Error
                )
            );
        }
        eException.Handle(ex => true);
        if (options.Value.TaskScheduler.PublishToLogger)
        {
            _logger.ZLogError(
                eException,
                $"Unhandled exception in TaskScheduler:{eException.Message}"
            );
        }
        if (options.Value.TaskScheduler.ForceApplicationCrash)
        {
            Dispatcher.UIThread.Invoke(() => throw eException);
        }
    }

    public void R3UnhandledException(Exception ex)
    {
        if (options.Value.R3.PublishToShell && shellHost.Shell is not null)
        {
            shellHost.Shell.ShowMessage(
                new ShellMessage("R3 Error", ex.ToString(), ShellErrorState.Error)
            );
        }
        if (options.Value.R3.PublishToLogger)
        {
            _logger.ZLogError(ex, $"Unhandled exception in R3: {ex.Message}");
        }
        if (options.Value.R3.ForceApplicationCrash)
        {
            Dispatcher.UIThread.Invoke(() => throw ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public interface IUnhandledExceptionHandler
{
    public void R3UnhandledException(Exception ex);
}
