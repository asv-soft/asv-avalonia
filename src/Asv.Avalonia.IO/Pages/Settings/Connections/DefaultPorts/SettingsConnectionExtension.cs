using System.Composition;
using Asv.IO;
using Material.Icons;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
public class SettingsConnectionExtension : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var serialMenuItem = new MenuItem(
            SerialProtocolPort.Scheme,
            $"Add {SerialProtocolPort.Info.Name}"
        );
        serialMenuItem.Icon = MaterialIconKind.SerialPort;
        serialMenuItem.Command = new BindableAsyncCommand(
            ProtocolPortCommand.StaticInfo.Id,
            serialMenuItem
        );
        var defaultConfig = SerialProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        serialMenuItem.CommandParameter = new CommandParameterAction(
            null,
            defaultConfig.AsUri().ToString(),
            CommandParameterActionType.Add
        );
        context.Menu.Add(serialMenuItem);
    }
}
