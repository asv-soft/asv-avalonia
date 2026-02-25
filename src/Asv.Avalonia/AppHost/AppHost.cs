using System.Diagnostics;
using System.Reflection;
using System.Text;
using Asv.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public class AppHost : AsyncDisposableWithCancel, IHost
{
    #region Static

    private static AppHost? _instance;
    public static AppHost Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AppHost)} not initialized. Please call {nameof(AppHost)}.{nameof(CreateBuilder)}().{nameof(Builder.Build)}() through first."
                );
            }

            return _instance;
        }
    }

    public static Builder CreateBuilder(string[] args)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException(
                $"{nameof(AppHost)} already configured. Only one instance allowed."
            );
        }

        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings
            {
#if DEBUG
                EnvironmentName = Environments.Development,
#else
                EnvironmentName = Environments.Production,
#endif
                Args = args,
            }
        );
        builder.Logging.ClearProviders();
        return new Builder(builder);
    }

    public static void HandleApplicationCrash(Exception e)
    {
        _instance
            ?.Services.GetService<ILoggerFactory>()
            ?.CreateLogger<AppHost>()
            .ZLogCritical(e, $"Application crashed: {e.Message}");

        var report = ExceptionReport.Build(e);
        Console.WriteLine(report);
        var dir = AppContext.BaseDirectory;
    }

    #endregion

    private readonly IHost _host;

    private AppHost(IHost host)
    {
        _host = host;
        _instance = this;
    }

    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    public IServiceProvider Services => _host.Services;

    public class Builder : IHostApplicationBuilder
    {
        private readonly HostApplicationBuilder _originBuilder;
        private readonly IHostApplicationBuilder _ifcBuilder;

        internal Builder(HostApplicationBuilder originBuilder)
        {
            _originBuilder = originBuilder;
            _ifcBuilder = originBuilder;
        }

        public void ConfigureContainer<TContainerBuilder>(
            IServiceProviderFactory<TContainerBuilder> factory,
            Action<TContainerBuilder>? configure = null
        )
            where TContainerBuilder : notnull =>
            _originBuilder.ConfigureContainer(factory, configure);

        public IDictionary<object, object> Properties => _ifcBuilder.Properties;
        public IConfigurationManager Configuration => _ifcBuilder.Configuration;
        public IHostEnvironment Environment => _ifcBuilder.Environment;
        public ILoggingBuilder Logging => _ifcBuilder.Logging;
        public IMetricsBuilder Metrics => _ifcBuilder.Metrics;
        public IServiceCollection Services => _ifcBuilder.Services;

        public IHost Build() => new AppHost(_originBuilder.Build());
    }
}
