using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class DeviceManagerRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder DeviceManager => builder.ModuleIo.Core.Services.DeviceManager;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder DeviceManager => new(builder);

        public ServicesRegistrations.Builder RegisterDeviceManager(
            Action<Builder>? configure = null
        )
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                builder.AppBuilder.Services.AddSingleton<IDeviceManager, NullDeviceManager>();
            }
            else
            {
                builder.AppBuilder.Services.AddSingleton<IDeviceManager, DeviceManager>();
            }

            builder.AppBuilder.Services.AddSingleton<IDeviceExplorer>(svc =>
                svc.GetRequiredService<IDeviceManager>().Explorer
            );
            builder.AppBuilder.Services.AddSingleton<IProtocolRouter>(svc =>
                svc.GetRequiredService<IDeviceManager>().Router
            );
            builder.AppBuilder.Services.AddSingleton<IProtocolFactory>(svc =>
                svc.GetRequiredService<IDeviceManager>().ProtocolFactory
            );
            return this;
        }

        public Builder RegisterDevice<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TDeviceManagerExtension
        >()
            where TDeviceManagerExtension : class, IDeviceManagerExtension
        {
            builder.AppBuilder.Services.AddSingleton<
                IDeviceManagerExtension,
                TDeviceManagerExtension
            >();
            return this;
        }
    }
}
