using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class TcpClientPortMenu : MenuItem
{
    public TcpClientPortMenu(ILoggerFactory loggerFactory)
        : base(TcpClientProtocolPort.Scheme, RS.SettingsConnectionTcpExtension_MenuItem_Header)
    {
        Icon = TcpClientPortViewModel.DefaultIcon;
        Command = new ReactiveCommand(AddPortAsync).DisposeItWith(Disposable);
    }

    private ValueTask AddPortAsync(Unit unit, CancellationToken cancel)
    {
        var defaultConfig = TcpClientProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionTcpPortExtension_DefaultConfig_Name;
        return this.FindParentOfType<SettingsConnectionViewModel>()?.AddPortAsync(defaultConfig)
            ?? ValueTask.CompletedTask;
    }
}
