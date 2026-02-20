using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class TcpClientPortMenu : MenuItem
{
    public TcpClientPortMenu(ILoggerFactory loggerFactory)
        : base(
            TcpClientProtocolPort.Scheme,
            RS.SettingsConnectionTcpExtension_MenuItem_Header,
            loggerFactory
        )
    {
        Icon = TcpClientPortViewModel.DefaultIcon;
        Command = new BindableAsyncCommand(PortCrudCommand.Id, this);
        var defaultConfig = TcpClientProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionTcpPortExtension_DefaultConfig_Name;
        CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
    }
}
