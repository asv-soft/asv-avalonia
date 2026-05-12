using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class UdpPortMenu : MenuItem
{
    public UdpPortMenu(ILoggerFactory loggerFactory)
        : base(UdpProtocolPort.Scheme, RS.SettingsConnectionUdpPortExtension_MenuItem_Header)
    {
        Icon = UdpPortViewModel.DefaultIcon;
        Command = new ReactiveCommand(AddPortAsync).DisposeItWith(Disposable);
    }

    private ValueTask AddPortAsync(Unit unit, CancellationToken cancel)
    {
        var defaultConfig = UdpProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionUdpPortExtension_DefaultConfig_Name;
        return this.FindParentOfType<SettingsConnectionViewModel>()?.AddPortAsync(defaultConfig)
            ?? ValueTask.CompletedTask;
    }
}
