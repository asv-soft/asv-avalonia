using System.Composition.Hosting;
using Avalonia.Controls;

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
            return containerConfiguration.WithAssemblies([typeof(IoModule).Assembly]);
        }

        var exceptionTypes = new List<Type>();
        if (AppHost.Instance.GetServiceOrDefault<IDeviceManager>() is { } deviceManager)
        {
            containerConfiguration.WithExport(deviceManager);
        }
        else
        {
            exceptionTypes.AddRange(
                [
                    typeof(HomePageDeviceListExtension),
                    typeof(SettingsPageExtension),
                    typeof(SettingsConnectionView),
                    typeof(SettingsConnectionViewModel),
                    typeof(PortView),
                    typeof(SerialPortView),
                    typeof(SerialPortViewModel),
                    typeof(SettingsConnectionSerialPortExtension),
                    typeof(TcpPortView),
                    typeof(TcpPortViewModel),
                    typeof(SettingsConnectionTcpPortExtension),
                    typeof(TcpServerPortView),
                    typeof(TcpServerPortViewModel),
                    typeof(SettingsConnectionTcpServerPortExtension),
                    typeof(UdpPortView),
                    typeof(UdpPortViewModel),
                    typeof(SettingsConnectionUdpPortExtension),
                    typeof(ConnectionRateStatusView),
                    typeof(ConnectionRateStatusViewModel),
                    typeof(PortCrudCommand),
                ]
            );
        }

        var iOTypes = typeof(IoModule).Assembly.GetTypes().Except(exceptionTypes);

        return containerConfiguration.WithParts(iOTypes);
    }
}
