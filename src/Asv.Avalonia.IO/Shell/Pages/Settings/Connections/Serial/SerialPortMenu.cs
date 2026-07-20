using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class SerialPortMenu : MenuItem
{
    public SerialPortMenu(ILoggerFactory loggerFactory)
        : base(SerialProtocolPort.Scheme, RS.SettingsConnectionSerialExtension_MenuItem_Header)
    {
        Icon = SerialPortViewModel.DefaultIcon;
        Command = new ReactiveCommand(AddPortAsync).DisposeItWith(Disposable);
    }

    private ValueTask AddPortAsync(Unit unit, CancellationToken cancel)
    {
        var defaultConfig = SerialProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionSerialPortExtension_DefaultConfig_Name;
        return this.FindParentOfType<SettingsConnectionViewModel>()
                ?.AddPortAsync(defaultConfig, cancel) ?? ValueTask.CompletedTask;
    }
}
