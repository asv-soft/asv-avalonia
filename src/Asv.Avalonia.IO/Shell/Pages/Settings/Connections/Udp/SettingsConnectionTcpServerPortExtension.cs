using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
public class SettingsConnectionUdpPortExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(
            UdpProtocolPort.Scheme,
            RS.SettingsConnectionUdpPortExtension_MenuItem_Header,
            loggerFactory
        );
        menu.Icon = UdpPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(PortCrudCommand.Id, menu);
        var defaultConfig = UdpProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionUdpPortExtension_DefaultConfig_Name;
        menu.CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
