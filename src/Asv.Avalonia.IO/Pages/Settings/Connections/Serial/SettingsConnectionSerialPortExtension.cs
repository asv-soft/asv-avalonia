using System.Composition;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
[method: ImportingConstructor]
public class SettingsConnectionSerialPortExtension(
    ILayoutService layoutService,
    ILoggerFactory loggerFactory
) : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(
            SerialProtocolPort.Scheme,
            RS.SettingsConnectionSerialExtension_MenuItem_Header,
            layoutService,
            loggerFactory
        );
        menu.Icon = SerialPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(PortCrudCommand.Id, menu);
        var defaultConfig = SerialProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = "New Serial";
        menu.CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
