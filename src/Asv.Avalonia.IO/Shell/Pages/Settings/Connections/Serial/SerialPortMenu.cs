using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;


public class SerialPortMenu : MenuItem
{
    public SerialPortMenu(ILoggerFactory loggerFactory)
        : base(
            SerialProtocolPort.Scheme,
            RS.SettingsConnectionSerialExtension_MenuItem_Header,
            loggerFactory)
    {
        Icon = SerialPortViewModel.DefaultIcon;
        Command = new BindableAsyncCommand(PortCrudCommand.Id, this);
        var defaultConfig = SerialProtocolPortConfig.CreateDefault();
        defaultConfig.IsEnabled = false;
        defaultConfig.Name = RS.SettingsConnectionSerialPortExtension_DefaultConfig_Name;
        CommandParameter = PortCrudCommand.CreateAddArg(defaultConfig);
    }
}