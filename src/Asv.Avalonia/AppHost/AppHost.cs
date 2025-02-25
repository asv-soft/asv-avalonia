using System.Composition.Hosting;
using Asv.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public class AppHost : AsyncDisposableWithCancel
{
    #region Static

    private static AppHost? _instance;

    public static AppHostBuilder CreateBuilder()
    {
        return CreateBuilder(configurationBuilder =>
        {
            configurationBuilder
                .AddJsonFile("app_settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(Environment.GetCommandLineArgs());
        });
    }

    public static AppHostBuilder CreateBuilder(Action<IConfigurationBuilder> configurationBuilder)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException(
                $"{nameof(AppHost)} already configured. Only one instance allowed."
            );
        }

        var builder = new ConfigurationBuilder();
        configurationBuilder(builder);
        return new AppHostBuilder(builder.Build());
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

    internal AppHost(ServiceProvider serviceProvider, IConfiguration config)
    {
        _instance = this;
        Services = serviceProvider;
        Configuration = config;
    }

    public IServiceProvider Services { get; }
    public IConfiguration Configuration { get; }

    public void HandleApplicationCrash(Exception e)
    {
        Services
            .GetService<ILoggerFactory>()
            ?.CreateLogger<AppHost>()
            .ZLogCritical(e, $"Application crashed: {e.Message}");
    }
}
