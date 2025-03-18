using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IConfiguration = Asv.Cfg.IConfiguration;

namespace Asv.Avalonia.Plugins;

public static class PluginManagerMixin
{
    /// <summary>
    /// Adds the Plugin Manager to the app core and configures it in the application builder.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the service to.</param>
    /// <param name="configure">Action to set up the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder UsePluginManager(
        this IHostApplicationBuilder builder,
        Action<PluginManagerBuilder>? configure = null
    )
    {
        var options = builder
            .Services.AddOptions<PluginManagerOptions>()
            .Bind(builder.Configuration.GetSection(PluginManagerOptions.Section));

        builder.Services.AddSingleton<IPluginManager, PluginManager>();
        var subBuilder = new PluginManagerBuilder();
        configure?.Invoke(subBuilder);
        subBuilder.Build(options);
        return builder;
    }
}
