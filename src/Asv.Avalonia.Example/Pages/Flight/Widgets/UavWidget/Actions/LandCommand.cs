using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;
[ExportCommand]
[Shared]
public class LandCommand : ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.land";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_Land,
        Description = RS.UavAction_Land_Description,
        Icon = MaterialIconKind.AeroplaneLanding,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<ICommandArg?> Execute(IRoutable context, ICommandArg newValue, CancellationToken cancel = default)
    {
        if (context is UavWidgetViewModel uav)
        {
            return InternalExecute(uav, newValue, cancel);
        }

        return default;
    }

    protected override ValueTask<ICommandArg?> InternalExecute(UavWidgetViewModel context, ICommandArg newValue, CancellationToken cancel)
    {
        var control = context.Device.GetMicroservice<ControlClient>();
        control?.EnsureGuidedMode(cancel: cancel);
        control?.DoLand(cancel);
        return ValueTask.FromResult<ICommandArg?>(newValue);
    }
}