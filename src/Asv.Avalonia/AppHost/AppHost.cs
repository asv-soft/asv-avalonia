using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public class AppHost : AsyncDisposableWithCancel, IHost
{
    #region Static

    private static AppHost? _instance;

    public static AppHostBuilder CreateBuilder(string[] args)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException(
                $"{nameof(AppHost)} already configured. Only one instance allowed."
            );
        }

        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        return new AppHostBuilder(builder);
    }

    public static AppHost Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AppHost)} not initialized. Please call {nameof(AppHost)}.{nameof(CreateBuilder)}().{nameof(AppHostBuilder.Build)}() through first."
                );
            }

            return _instance;
        }
    }

    #endregion

    private readonly IHost _host;

    internal AppHost(IHost host)
    {
        _host = host;
        _instance = this;
    }

    public T GetService<T>()
        where T : notnull
    {
        return _host.Services.GetRequiredService<T>();
    }

    public void HandleApplicationCrash(Exception e)
    {
        GetService<ILoggerFactory>()
            .CreateLogger<AppHost>()
            .ZLogCritical(e, $"Application crashed: {e.Message}");
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _host.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return _host.StopAsync(cancellationToken);
    }

    public IServiceProvider Services => _host.Services;
}
