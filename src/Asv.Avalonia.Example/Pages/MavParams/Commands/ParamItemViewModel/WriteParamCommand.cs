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
        Name = "Write param",
        Description = "Command that writes the param to the drone",
        Icon = MaterialIconKind.Upload,
        DefaultHotKey = null, // TODO: make a key bind when new key listener system appears
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        ParamItemViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        context.WriteImpl(cancel).SafeFireAndForget();
        return default;
    }
}
