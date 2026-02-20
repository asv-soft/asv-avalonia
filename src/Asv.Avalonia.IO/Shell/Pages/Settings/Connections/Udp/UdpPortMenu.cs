using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class UdpPortMenu : MenuItem
{
    public UdpPortMenu(ILoggerFactory loggerFactory)
        : base(
            UdpProtocolPort.Scheme,
            RS.SettingsConnectionUdpPortExtension_MenuItem_Header,
            loggerFactory)
    {
        Icon = UdpPortViewModel.DefaultIcon;
        Command = new BindableAsyncCommand(PortCrudCommand.Id, this);
        var defaultConfig = UdpProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionUdpPortExtension_DefaultConfig_Name;
        CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
    }
}

