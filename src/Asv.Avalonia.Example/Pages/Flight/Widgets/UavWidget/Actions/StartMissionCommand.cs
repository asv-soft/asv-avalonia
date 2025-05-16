using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportCommand]
[Shared]
public class StartMissionCommand : ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.uav.start";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_StartMission,
        Description = RS.UavAction_StartMission_Description,
        Icon = MaterialIconKind.MapMarkerPath,
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
        var control = context.Device.GetMicroservice<ControlClient>();
        var mission = context.Device.GetMicroservice<MissionClientEx>();
        if (control is null || mission is null)
        {
            return default;
        }

        mission.SetCurrent(0, cancel);
        control.SetAutoMode(cancel);
        return default;
    }
}
