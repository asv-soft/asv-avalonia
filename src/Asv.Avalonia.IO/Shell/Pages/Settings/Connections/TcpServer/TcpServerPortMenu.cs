using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class TcpServerPortMenu : MenuItem
{
    public TcpServerPortMenu(ILoggerFactory loggerFactory)
        : base(
            TcpServerProtocolPort.Scheme,
            RS.SettingsConnectionTcpServerPortExtension_MenuItem_Header,
            loggerFactory
        )
    {
        Icon = TcpServerPortViewModel.DefaultIcon;
        Command = new BindableAsyncCommand(PortCrudCommand.Id, this);
        var defaultConfig = TcpServerProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionTcpServerPortExtension_DefaultConfig_Name;
        CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
    }
}
