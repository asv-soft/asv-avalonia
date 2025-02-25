using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class AppHostBuilder
{
    private const string LogLevelConfigSection = "LogLevel";

    private Action<AppHost> _afterSetupCallback = _ => { };
    private Action<AppHostBuilder> _beforeSetupCallback = _ => { };
    private Action<ILoggingBuilder> _logConfigCallback;
    private readonly ServiceCollection _services = new();

    internal AppHostBuilder(IConfigurationRoot appConfig)
    {
        AppConfig = appConfig;
        var minLogLevel = LogLevel.Information;
        if (!string.IsNullOrWhiteSpace(appConfig[LogLevelConfigSection]))
        {
            if (
                Enum.TryParse<LogLevel>(
                    appConfig[LogLevelConfigSection] ?? nameof(LogLevel.Information),
                    true,
                    out var logLevel
                )
            )
            {
                minLogLevel = logLevel;
            }
        }

        _logConfigCallback += builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(minLogLevel);
        };
    }

    public IServiceCollection Services => _services;
    public IConfiguration AppConfig { get; }

    public AppHostBuilder SetupLogging(Action<ILoggingBuilder> callback)
    {
        _logConfigCallback += callback;
        return this;
    }

    public AppHostBuilder BeforeSetup(Action<AppHostBuilder> callback)
    {
        _beforeSetupCallback += callback;
        return this;
    }

    public AppHostBuilder AfterSetup(Action<AppHost> callback)
    {
        _afterSetupCallback += callback;
        return this;
    }

    public AppHost Build()
    {
        _beforeSetupCallback(this);
        var logFactory = LoggerFactory.Create(_logConfigCallback);
        _services.AddSingleton(logFactory);
        var host = new AppHost(_services.BuildServiceProvider(), AppConfig);
        _afterSetupCallback(host);
        return host;
    }
}
