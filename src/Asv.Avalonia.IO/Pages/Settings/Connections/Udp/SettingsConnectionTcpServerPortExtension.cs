using System.Composition;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
[method: ImportingConstructor]
public class SettingsConnectionUdpPortExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(UdpProtocolPort.Scheme, "UDP", loggerFactory);
        menu.Icon = UdpPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(PortCrudCommand.Id, menu);
        var defaultConfig = UdpProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = "New UDP";
        menu.CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
