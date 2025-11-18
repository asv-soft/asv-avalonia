using System.Composition.Hosting;
using Avalonia.Controls;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.IO;

public sealed class IoModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.IO";
    public static IoModule Instance { get; } = new();

    private IoModule() { }

    public string ModuleName => Name;
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromIoModule(
        this ContainerConfiguration containerConfiguration
    )
    {
        if (Design.IsDesignMode)
        {
            containerConfiguration.WithExport(NullDeviceManager.Instance);
        }
        else
        {
            var options = AppHost.Instance.GetService<IOptions<IoModuleOptions>>().Value;
            if (!options.EnableDevices)
            {
                return containerConfiguration;
            }

            containerConfiguration.WithExport(AppHost.Instance.GetService<IDeviceManager>());
        }

        return containerConfiguration
            .WithPart<HomePageDeviceListExtension>()
            .WithPart<SettingsPageExtension>()
            .WithPart<SettingsConnectionView>()
            .WithPart<SettingsConnectionViewModel>()
            .WithPart<PortVIew>()
            .WithPart<SerialPortView>()
            .WithPart<SerialPortViewModel>()
            .WithPart<SettingsConnectionSerialPortExtension>()
            .WithPart<TcpPortView>()
            .WithPart<TcpPortViewModel>()
            .WithPart<SettingsConnectionTcpPortExtension>()
            .WithPart<TcpServerPortView>()
            .WithPart<TcpServerPortViewModel>()
            .WithPart<SettingsConnectionTcpServerPortExtension>()
            .WithPart<UdpPortView>()
            .WithPart<UdpPortViewModel>()
            .WithPart<SettingsConnectionUdpPortExtension>()
            .WithPart<ConnectionRateStatusView>()
            .WithPart<ConnectionRateStatusViewModel>();
    }
}
