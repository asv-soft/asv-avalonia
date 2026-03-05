using Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public static class AppHost
{
    #region Singleton

    private static IHost? _instance;
    public static IHost Instance
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

    #endregion

    
    
    #region Builder

    public static Builder CreateBuilder(HostApplicationBuilderSettings? settings = null)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException(
                $"{nameof(AppHost)} already configured. Only one instance allowed."
            );
        }
        settings ??= new HostApplicationBuilderSettings
        {
            Args = Environment.GetCommandLineArgs(),
        };
        var builder = Host.CreateApplicationBuilder(settings);

        return new Builder(builder);
    }

    public class Builder : IHostApplicationBuilder
    {
        private readonly HostApplicationBuilder _originBuilder;
        private readonly IHostApplicationBuilder _ifcBuilder;
        private const string PostConfigureCallbackKey = "PostConfigureCallback";

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
        
        public IHost Build()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AppHost)} already configured. Only one instance allowed."
                );
            }
            _originBuilder.ExecutePostConfigureCallbacks();
            return _instance = _originBuilder.Build();
        }
    }

    #endregion

 
    public static void HandleApplicationCrash(Exception e)
    {
        _instance
            ?.Services.GetService<ILoggerFactory>()
            ?.CreateLogger(nameof(AppHost))
            .ZLogCritical(e, $"Application crashed: {e.Message}");

        ExceptionReport.WriteToFile(AppContext.BaseDirectory, e, out var content);
        Console.WriteLine(content);
    }
}
