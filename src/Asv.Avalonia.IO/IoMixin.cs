using Asv.IO;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class IoMixin
{
    extension(SettingsPageMixin.Builder builder)
    {
        public SettingsPageMixin.Builder UseConnectionsSettings(Action<ConnectionSettingsBuilder>? configure = null)
        {
            builder.Parent.Parent.Parent.Commands.Register<PortCrudCommand>();
            
            builder.Parent.Parent.Parent.ViewLocator.RegisterViewFor<PortViewModel, PortView>();
            
            builder.AddSubPage<SettingsConnectionViewModel, SettingsConnectionView, SettingsConnectionTreePageMenu>(
                SettingsConnectionViewModel.SubPageId);
            
            builder.Parent.Parent.Parent.Extensions.Register<ISettingsConnectionSubPage, SettingsConnectionDefaultMenuExtension>();

            builder.Parent.Parent.Parent.Extensions.Register<IHomePage, HomePageDeviceListExtension>();
            
            configure ??= b => b.UseDefault();
            configure(new ConnectionSettingsBuilder(builder));
            return builder;
        }
        
        public ConnectionSettingsBuilder Connections => new(builder);
    }

    public class ConnectionSettingsBuilder(SettingsPageMixin.Builder builder)
    {
        public ConnectionSettingsBuilder UseDefault()
        {
            return UseSerialPort()
                .UseTcpClientPort()
                .UseTcpServerPort()
                .UseUdpPort();
        }
        public ConnectionSettingsBuilder UseUdpPort()
        {
            return UsePort<UdpPortViewModel, UdpPortView, UdpPortMenu>(UdpProtocolPort.Scheme);
        }
        public ConnectionSettingsBuilder UseSerialPort()
        {
            return UsePort<SerialPortViewModel, SerialPortView, SerialPortMenu>(SerialProtocolPort.Scheme);
        }
        
        public ConnectionSettingsBuilder UseTcpClientPort()
        {
            return UsePort<TcpClientPortViewModel, TcpClientPortView, TcpClientPortMenu>(TcpClientProtocolPort.Scheme);
        }
        
        public ConnectionSettingsBuilder UseTcpServerPort()
        {
            return UsePort<TcpServerPortViewModel, TcpServerPortView, TcpServerPortMenu>(TcpServerProtocolPort.Scheme);
        }

        public ConnectionSettingsBuilder UsePort<TViewModel, TView, TMenu>(string portScheme)
            where TViewModel : class, IPortViewModel
            where TView : Control
            where TMenu : class, IMenuItem 
        {
            builder.Parent.Parent.Parent.ViewLocator.RegisterViewFor<TViewModel, TView>();
            builder.Parent.Parent.Parent.Services.AddKeyedTransient<IPortViewModel, TViewModel>(portScheme);
            builder.Parent.Parent.Parent.Services.AddKeyedTransient<IMenuItem, TMenu>(SettingsConnectionDefaultMenuExtension.Contract);
            return this;
        }
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
        private void InternalUseIo()
        {
            
        }

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
        public IHostApplicationBuilder RegisterDefault()
        {
            builder.Shell.Pages.Settings.UseConnectionsSettings();
            builder.Shell.Status.UseConnectionStatus();
            return builder;
        }

        public IHostApplicationBuilder Parent => builder;

        public Builder RegisterDevice<TDeviceManagerExtension>()
            where TDeviceManagerExtension : class, IDeviceManagerExtension
        {
            builder.Services.AddSingleton<IDeviceManagerExtension, TDeviceManagerExtension>();
            return this;
        }
    }
}
