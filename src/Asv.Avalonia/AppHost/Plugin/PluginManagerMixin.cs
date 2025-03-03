using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IConfiguration = Asv.Cfg.IConfiguration;

namespace Asv.Avalonia;

public static class PluginManagerMixin
{
    /// <summary>
    /// Adds the Plugin Manager to the app core and configures it in the application builder.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the service to.</param>
    /// <param name="configure">Action to set up the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static AppHostBuilder UsePluginManager(
        this AppHostBuilder builder,
        Action<PluginManagerBuilder>? configure = null
    )
    {
        var options = builder
            .Services.AddOptions<BuilderPluginManagerConfig>()
            .Bind(builder.AppConfig.GetSection(BuilderPluginManagerConfig.Section));

        builder.Services.AddSingleton<IPluginManager>(provider =>
        {
            var cfg = builder.AppConfig.Get<BuilderPluginManagerConfig>();
            var appPath = provider.GetService<IAppPath>();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var cfgSvc = provider.GetRequiredService<IConfiguration>();

            if (appPath is null)
            {
                throw new NullReferenceException($"{nameof(appPath)} is null");
            }

            if (loggerFactory is null)
            {
                throw new NullReferenceException($"{nameof(loggerFactory)} is null");
            }

            if (cfg is null)
            {
                throw new Exception(
                    $"Missing configuration section {BuilderPluginManagerConfig.Section}"
                );
            }

            var pm = new PluginManager(
                appPath.UserDataFolder,
                cfg.ApiPackageName,
                cfg.NugetPluginPrefix,
                cfg.ApiVersion,
                cfgSvc,
                loggerFactory
            );
            cfg.Servers.ForEach(s => pm.AddServer(s));

            return pm;
        });

        var subBuilder = new PluginManagerBuilder();
        configure?.Invoke(subBuilder);
        subBuilder.Build(options);
        return builder;
    }
}
