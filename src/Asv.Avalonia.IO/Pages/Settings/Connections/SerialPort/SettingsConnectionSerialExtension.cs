using System.Composition;
using Asv.IO;
using Material.Icons;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
public class SettingsConnectionSerialExtension : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(SerialProtocolPort.Scheme, $"Add {SerialProtocolPort.Info.Name}");
        menu.Icon = SerialPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(ProtocolPortCommand.Id, menu);
        var defaultConfig = SerialProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        menu.CommandParameter = ProtocolPortCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
