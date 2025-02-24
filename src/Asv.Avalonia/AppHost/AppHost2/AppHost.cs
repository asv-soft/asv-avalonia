using Asv.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class AsvHostBuilder
{
    private Action<AsvHost> _afterSetupCallback = _ => { };
    private Action<AsvHostBuilder> _beforeSetupCallback = _ => { };
    private readonly ServiceCollection _services = new();

    internal AsvHostBuilder(IConfigurationRoot configuration)
    {
        Configuration = configuration;
    }

    public IServiceCollection Services => _services;
    public IConfiguration Configuration { get; }

    public AsvHostBuilder BeforeSetup(Action<AsvHostBuilder> callback)
    {
        _beforeSetupCallback += callback;
        return this;
    }

    public AsvHostBuilder AfterSetup(Action<AsvHost> callback)
    {
        _afterSetupCallback += callback;
        return this;
    }

    public AsvHost Build()
    {
        _beforeSetupCallback(this);
        var host = new AsvHost(_services.BuildServiceProvider(), Configuration);
        _afterSetupCallback(host);
        return host;
    }
}

public class AsvHost : AsyncDisposableWithCancel
{
    #region Static

    private static AsvHost? _instance;
    public static AsvHostBuilder CreateBuilder()
    {
        return CreateBuilder(configurationBuilder =>
        {
            configurationBuilder
                .AddJsonFile("app_settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(Environment.GetCommandLineArgs());
        });
    }

    public static AsvHostBuilder CreateBuilder(Action<IConfigurationBuilder> configurationBuilder)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException(
                $"{nameof(AsvHost)} already configured. Only one instance allowed."
            );
        }
        
        var builder = new ConfigurationBuilder();
        configurationBuilder(builder);
        return new AsvHostBuilder(builder.Build());
    }

    public static AsvHost Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AppHost)} not initialized. Please call {nameof(AsvHost)}.{nameof(CreateBuilder)}().{nameof(AppHostBuilder.Build)}() through first."
                );
            }

            return _instance;
        }
    }

    #endregion

    internal AsvHost(ServiceProvider serviceProvider, IConfiguration config)
    {
        _instance = this;
        Services = serviceProvider;
        Configuration = config;
    }

    public IServiceProvider Services { get; }
    public IConfiguration Configuration { get; }

    public void HandleApplicationCrash(Exception exception)
    {
        
    }

    
}
