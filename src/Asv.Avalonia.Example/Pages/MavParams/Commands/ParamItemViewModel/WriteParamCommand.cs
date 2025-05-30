using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportCommand]
public class WriteParamCommand : ContextCommand<ParamItemViewModel>
{
    public const string Id = $"{BaseId}.params.item.write";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.WritePatamCommand_CommandInfo_Name,
        Description = RS.WriteParamCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Upload,
        HotKeyInfo = new HotKeyInfo
        {
            DefaultHotKey = null, // TODO: make a key bind when new key listener system appears
        },
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandArg?> InternalExecute(
        ParamItemViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.WriteImpl(cancel);
        return null;
    }
}
