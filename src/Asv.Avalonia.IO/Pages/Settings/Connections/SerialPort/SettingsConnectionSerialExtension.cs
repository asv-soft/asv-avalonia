using Asv.IO;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
public class SettingsConnectionSerialExtension : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(SerialProtocolPort.Scheme, $"{RS.SettingsConnectionTcpClientExtension_AddSerial_ButtonContent}");
        menu.Icon = SerialPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(ProtocolPortCommand.Id, menu);
        var defaultConfig = SerialProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        menu.CommandParameter = ProtocolPortCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
