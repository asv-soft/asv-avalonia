using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.IO;
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
        Description = "Execute take off action", // TODO:Localize
        Icon = MaterialIconKind.AeroplaneTakeoff,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    private IDeviceManager _deviceManager;

    [ImportingConstructor]
    public TakeOffCommand(IDeviceManager connectionService)
    {
        _deviceManager = connectionService;
    }

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<ICommandArg?> Execute(
        IRoutable context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        return InternalExecute(newValue, cancel);
    }

    protected override ValueTask<ICommandArg?> InternalExecute(
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        if (
            newValue is ActionCommandArg keyValuePair
            && double.TryParse(keyValuePair.Value, out var altitude)
        )
        {
            var device = _deviceManager
                .Explorer.Devices.First(_ => _.Value.Id.AsString() == keyValuePair.Id)
                .Value;
            device.WaitUntilConnectAndInit(100, TimeProvider.System);
            var controlClient = device.GetMicroservice<ControlClient>();
            controlClient?.SetGuidedMode(cancel);
            controlClient?.TakeOff(altitude, cancel);
        }

        return default;
    }
}
