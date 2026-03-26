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
        if (!options.Value.IsEnabled == false)
        {
            _logger.LogInformation("Unhandled exception handler is disabled");
            return Task.CompletedTask;
        }

        _logger.ZLogInformation($"Unhandled exception handler is enabled: {options}");
        R3.ObservableSystem.RegisterUnhandledExceptionHandler(R3UnhandledException);
        TaskScheduler.UnobservedTaskException += (_, e) =>
            TaskSchedulerUnhandledException(e.Exception);
        return Task.CompletedTask;
    }

    public void TaskSchedulerUnhandledException(AggregateException eException)
    {
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
