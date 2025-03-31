using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Avalonia.Example;
[ExportCommand]
[Shared]
public class AutoModeCommand: ContextCommand<UavWidgetViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.auto";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UavAction_AutoMode,
        Description = RS.UavAction_AutoMode_Description,
        Icon = MaterialIconKind.Automatic,
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
        control?.SetAutoMode(cancel);
        return ValueTask.FromResult<ICommandArg?>(newValue);
    }
}