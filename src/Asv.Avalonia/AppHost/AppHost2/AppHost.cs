using Asv.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public class AsvHostBuilder
{
    private Action<AsvHost> _afterSetupCallback = _ => { };
    private Action<ServiceProvider> _beforeSetupCallback = _ => { };

    internal AsvHostBuilder() { }

    public ServiceCollection Services { get; } = new();
    public IConfigurationBuilder Configuration { get; set; } = new ConfigurationBuilder();

    public AsvHostBuilder BeforeSetup(Action<ServiceProvider> callback)
    {
        _beforeSetupCallback += callback;
        return this;
    }

    public AsvHostBuilder AfterSetup(Action<AsvHost> callback)
    {
        _afterSetupCallback += callback;
        return this;
    }

    internal AsvHost Build()
    {
        var provider = Services.BuildServiceProvider();
        _beforeSetupCallback(provider);
        var host = new AsvHost(provider);
        _afterSetupCallback(host);
        return host;
    }
}

public class AsvHost : AsyncDisposableWithCancel
{
    #region Static

    private static AsvHost? _instance = null;

    public static AsvHost Configure(Action<AsvHostBuilder> options)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException(
                $"{nameof(AsvHost)} already configured. Only one instance allowed."
            );
        }

        var builder = new AsvHostBuilder();
        options(builder);
        return _instance = builder.Build();
    }

    public static AsvHost Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AppHost)} not initialized. Please call {nameof(AsvHost)}.{nameof(Configure)} through first."
                );
            }

            return _instance;
        }
    }

    #endregion

    internal AsvHost(ServiceProvider serviceProvider)
    {
        _instance = this;
        Services = serviceProvider;
    }

    public ServiceProvider Services { get; }
}
