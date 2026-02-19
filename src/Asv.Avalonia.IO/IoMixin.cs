using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class IoMixin
{
    extension(SettingsPageMixin.Builder builder)
    {
        public SettingsPageMixin.Builder UseConnections() { }
    }

    extension(ShellMixin.StatusBuilder builder)
    {
        public ShellMixin.StatusBuilder UseConnectionStatus()
        {
            return builder.Register<ConnectionRateStatusViewModel, ConnectionRateStatusView>();
        }
    }

    extension(IHostApplicationBuilder builder)
    {
        private void InternalUseIo() { }

        public IHostApplicationBuilder UseModuleIo(Action<Builder>? configure = null)
        {
            builder.InternalUseIo();
            configure ??= (b) => b.RegisterDefault();
            configure(new Builder(builder));
            builder.Services.AddSingleton<IDeviceManager, DeviceManager>();
            return builder;
        }

        public IHostApplicationBuilder UseModuleDesignTimeIo(Action<Builder>? configure = null)
        {
            builder.Services.AddSingleton<IDeviceManager, NullDeviceManager>();
            return builder;
        }

        public Builder ModuleIo => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public void RegisterDefault() { }

        public IHostApplicationBuilder Parent => builder;

        public Builder Register<TDeviceManagerExtension>()
            where TDeviceManagerExtension : class, IDeviceManagerExtension
        {
            builder.Services.AddSingleton<IDeviceManagerExtension, TDeviceManagerExtension>();
            return this;
        }
    }
}
