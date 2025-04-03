using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;

public class GuidedModeCommand : ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.guided";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_GuidedMode,
        Description = RS.UavAction_GuidedMode_Description,
        Icon = MaterialIconKind.Controller,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(UavWidgetViewModel context, ICommandArg newValue, CancellationToken cancel)
    {
        var control = context.Device.GetMicroservice<ControlClient>();
        control?.SetAutoMode(cancel);
        return ValueTask.FromResult<ICommandArg?>(newValue);
    }
}