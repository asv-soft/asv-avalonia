using System.Composition;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
[method: ImportingConstructor]
public class SettingsConnectionTcpServerPortExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(
            TcpServerProtocolPort.Scheme,
            RS.SettingsConnectionTcpServerPortExtension_MenuItem_Header,
            loggerFactory
        );

        menu.Icon = TcpServerPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(PortCrudCommand.Id, menu);
        var defaultConfig = TcpServerProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionTcpServerPortExtension_DefaultConfig_Name;
        menu.CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
