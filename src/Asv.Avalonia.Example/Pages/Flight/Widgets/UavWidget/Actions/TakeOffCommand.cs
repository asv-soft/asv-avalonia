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
public class TakeOffCommand : ContextCommand<UavWidgetViewModel>
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
        if (context is UavWidgetViewModel uav)
        {
            return InternalExecute(uav, newValue, cancel);      
        }

        return default;
    }

    protected override ValueTask<ICommandArg?> InternalExecute(UavWidgetViewModel context, ICommandArg newValue,
        CancellationToken cancel)
    {
        if (newValue is DoubleCommandArg value)
        {
            var device = context.Device;
            device.WaitUntilConnectAndInit(100, TimeProvider.System);
            var controlClient = device.GetMicroservice<ControlClient>();
            controlClient?.SetGuidedMode(cancel);
            controlClient?.TakeOff(value.Value, cancel);
        }

        return default;
    }
}