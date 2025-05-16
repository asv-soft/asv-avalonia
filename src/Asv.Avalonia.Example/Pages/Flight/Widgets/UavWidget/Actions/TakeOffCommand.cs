using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportCommand]
[Shared]
public class TakeOffCommand : ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.uav.takeOff";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_TakeOff,
        Description = RS.UavAction_TakeOff_Description,
        Icon = MaterialIconKind.AeroplaneTakeoff,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        UavWidgetViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not DoubleCommandArg value)
        {
            throw new InvalidCastException("Invalid value type. CommandArg must be a double");
        }

        var device = context.Device;
        var controlClient = device.GetMicroservice<ControlClient>();
        controlClient?.SetGuidedMode(cancel);
        controlClient?.TakeOff(value.Value, cancel);

        return default;
    }
}
