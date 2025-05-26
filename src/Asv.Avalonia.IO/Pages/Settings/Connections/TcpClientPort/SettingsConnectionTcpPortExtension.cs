﻿using Asv.IO;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsConnectionSubPage>]
public class SettingsConnectionTcpPortExtension : IExtensionFor<ISettingsConnectionSubPage>
{
    public void Extend(ISettingsConnectionSubPage context, CompositeDisposable contextDispose)
    {
        var menu = new MenuItem(
            TcpClientProtocolPort.Scheme,
            $"{RS.SettingsConnectionTcpExtension_MenuItem_Header}"
        );
        menu.Icon = TcpPortViewModel.DefaultIcon;
        menu.Command = new BindableAsyncCommand(ProtocolPortCommand.Id, menu);
        var defaultConfig = TcpClientProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        menu.CommandParameter = ProtocolPortCommand.CreateAddArg(defaultConfig);
        context.Menu.Add(menu);
    }
}
