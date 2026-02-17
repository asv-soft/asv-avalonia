using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
public class SettingsConnectionTcpPortExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(
            TcpClientProtocolPort.Scheme,
            RS.SettingsConnectionTcpExtension_MenuItem_Header,
            loggerFactory
        );
        menu.Icon = TcpPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(PortCrudCommand.Id, menu);
        var defaultConfig = TcpClientProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionTcpPortExtension_DefaultConfig_Name;
        menu.CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
