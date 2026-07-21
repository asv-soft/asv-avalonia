using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class TcpServerPortMenu : MenuItem
{
    public TcpServerPortMenu(ILoggerFactory loggerFactory)
        : base(
            TcpServerProtocolPort.Scheme,
            RS.SettingsConnectionTcpServerPortExtension_MenuItem_Header
        )
    {
        Icon = TcpServerPortViewModel.DefaultIcon;
        Command = new ReactiveCommand(AddPortAsync).DisposeItWith(Disposable);
    }

    private ValueTask AddPortAsync(Unit unit, CancellationToken cancel)
    {
        var defaultConfig = TcpServerProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionTcpServerPortExtension_DefaultConfig_Name;
        return this.FindParentOfType<SettingsConnectionViewModel>()
                ?.AddPortAsync(defaultConfig, cancel) ?? ValueTask.CompletedTask;
    }
}
