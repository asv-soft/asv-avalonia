using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportCommand]
[Shared]
public class TakeOffCommand : NoContextCommand
{
    #region Static

    public const string Id = $"{BaseId}.takeOff";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "TakeOff",
        Description = "Execute take off action",
        Icon = MaterialIconKind.AeroplaneTakeoff,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    private IMavlinkConnectionService _mavlinkConnectionService;
    
    [ImportingConstructor]
    public TakeOffCommand(IMavlinkConnectionService connectionService)
    {
        _mavlinkConnectionService = connectionService;
    }

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<IPersistable?> Execute(IRoutable context, IPersistable newValue, CancellationToken cancel)
    {
        return InternalExecute(newValue, cancel);     
    }

    protected override ValueTask<IPersistable?> InternalExecute(IPersistable newValue, CancellationToken cancel)
    {
        if (newValue is Persistable<KeyValuePair<DeviceId, double>> keyValuePair)
        {
            var device = _mavlinkConnectionService.DevicesExplorer.Devices.First(_ => _.Value.Id == keyValuePair.Value.Key).Value;
            device.WaitUntilConnectAndInit(100, TimeProvider.System);
           var controlClient = device.GetMicroservice<ControlClient>();
           controlClient?.SetGuidedMode(cancel);
           controlClient?.TakeOff(keyValuePair.Value.Value, cancel);
        }
        
        return default;     
    }
}