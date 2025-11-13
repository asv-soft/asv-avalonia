using System.Composition.Hosting;
using Avalonia.Controls;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.IO;

public sealed class IoModule : IExportInfo, IExportModule<IoModuleOptions>
{
    public const string Name = "Asv.Avalonia.IO";
    public static IoModule Instance { get; } = new();

    private IoModule() { }

    public string ModuleName => Name;

    public ContainerConfiguration ExportTypes(
        ContainerConfiguration containerConfiguration,
        IOptions<IoModuleOptions> options
    )
    {
        TryExportDeviceItems(containerConfiguration, options.Value);

        return containerConfiguration;
    }

    private void TryExportDeviceItems(
        ContainerConfiguration containerConfiguration,
        IoModuleOptions options
    )
    {
        if (!options.EnableDevices)
        {
            return;
        }

        if (Design.IsDesignMode)
        {
            containerConfiguration.WithExport(NullDeviceManager.Instance);
        }
        else
        {
            containerConfiguration.WithExport(AppHost.Instance.GetService<IDeviceManager>());
        }

        containerConfiguration
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
