using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class AppHostBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _originBuilder;
    private readonly IHostApplicationBuilder _ifcBuilder;

    internal AppHostBuilder(HostApplicationBuilder originBuilder)
    {
        _originBuilder = originBuilder;
        _ifcBuilder = originBuilder;
    }

    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    )
        where TContainerBuilder : notnull => _originBuilder.ConfigureContainer(factory, configure);

    public IDictionary<object, object> Properties => _ifcBuilder.Properties;
    public IConfigurationManager Configuration => _ifcBuilder.Configuration;
    public IHostEnvironment Environment => _ifcBuilder.Environment;
    public ILoggingBuilder Logging => _ifcBuilder.Logging;
    public IMetricsBuilder Metrics => _ifcBuilder.Metrics;
    public IServiceCollection Services => _ifcBuilder.Services;

    public IHost Build() => new AppHost(_originBuilder.Build());
}
