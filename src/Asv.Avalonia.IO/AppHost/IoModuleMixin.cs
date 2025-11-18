using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.IO;

public static class IoModuleMixin
{
    /// <summary>
    /// Adds the IoModule config to the app core and configures it in the application builder.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the service to.</param>
    /// <param name="configure">Action to set up the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder UseIo(
        this IHostApplicationBuilder builder,
        Action<IoModuleOptionsBuilder>? configure = null
    )
    {
        var defaultOptions = builder
            .Configuration.GetSection(IoModuleOptions.ConfigurationSection)
            .Get<IoModuleOptions>();

        var optionsBuilder = defaultOptions is null
            ? new IoModuleOptionsBuilder()
            : new IoModuleOptionsBuilder(defaultOptions);

        if (configure is null)
        {
            return builder;
        }

        configure.Invoke(optionsBuilder);

        var options = optionsBuilder.Build();

        ApplyDevices(builder, options);

        builder.Services.AddSingleton(Options.Create(options));

        return builder;
    }

    private static void ApplyDevices(IHostApplicationBuilder builder, IoModuleOptions loggerOptions)
    {
        if (!loggerOptions.EnableDevices)
        {
            return;
        }

        builder.Services.AddSingleton<IDeviceManager, DeviceManager>();
    }
}
