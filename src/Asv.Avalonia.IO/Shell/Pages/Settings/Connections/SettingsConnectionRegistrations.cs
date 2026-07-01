using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia;
using Asv.IO;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.IO;

public static class SettingsConnectionRegistrations
{
    extension(SettingsPageRegistrations.Builder builder)
    {
        public Builder Connections => new(builder);

        public SettingsPageRegistrations.Builder RegisterConnectionSettingsSubPage(
            Action<Builder>? configure = null
        )
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<PortViewModel, PortView>();

            builder.AppBuilder.Settings.AddSubPage<
                SettingsConnectionViewModel,
                SettingsConnectionView,
                SettingsConnectionTreePageMenu
            >(SettingsConnectionViewModel.SubPageId);

            builder.AppBuilder.Extensions.Register<
                ISettingsConnectionSubPage,
                SettingsConnectionDefaultMenuExtension
            >();

            builder.AppBuilder.Extensions.Register<IHomePage, HomePageDeviceListExtension>();

            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(SettingsPageRegistrations.Builder builder)
    {
        public Builder RegisterDefault()
        {
            return RegisterSerialPort()
                .RegisterTcpClientPort()
                .RegisterTcpServerPort()
                .RegisterUdpPort();
        }

        public Builder RegisterUdpPort()
        {
            return RegisterPort<UdpPortViewModel, UdpPortView, UdpPortMenu>(UdpProtocolPort.Scheme);
        }

        public Builder RegisterSerialPort()
        {
            return RegisterPort<SerialPortViewModel, SerialPortView, SerialPortMenu>(
                SerialProtocolPort.Scheme
            );
        }

        public Builder RegisterTcpClientPort()
        {
            return RegisterPort<TcpClientPortViewModel, TcpClientPortView, TcpClientPortMenu>(
                TcpClientProtocolPort.Scheme
            );
        }

        public Builder RegisterTcpServerPort()
        {
            return RegisterPort<TcpServerPortViewModel, TcpServerPortView, TcpServerPortMenu>(
                TcpServerProtocolPort.Scheme
            );
        }

        public Builder RegisterPort<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMenu
        >(string portScheme)
            where TViewModel : class, IPortViewModel
            where TView : Control
            where TMenu : class, IMenuItem
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<TViewModel, TView>();
            builder.AppBuilder.Services.AddKeyedTransient<IPortViewModel, TViewModel>(portScheme);
            builder.AppBuilder.Services.AddKeyedTransient<
                ViewModelFactoryDelegate<IPortViewModel, IProtocolPort>
            >(
                portScheme,
                (services, _) =>
                    protocolPort =>
                    {
                        ArgumentNullException.ThrowIfNull(protocolPort);
                        return ActivatorUtilities.CreateInstance<TViewModel>(
                            services,
                            protocolPort
                        );
                    }
            );
            builder.AppBuilder.Services.AddKeyedTransient<IMenuItem, TMenu>(
                SettingsConnectionDefaultMenuExtension.Contract
            );
            return this;
        }
    }
}
