using Material.Icons;

namespace Asv.Avalonia;

public class TextSearchCommand : ContextCommand<ISupportTextSearch, StringArg>
{
    public const string Id = $"{BaseId}.search.text";

    public override ICommandInfo Info =>
        new CommandInfo
        {
            Id = Id,
            Name = RS.TextSearchCommand_CommandInfo_Name,
            Description = RS.TextSearchCommand_CommandInfo_Description,
            Icon = MaterialIconKind.Search,
            DefaultHotKey = null,
        };

    public override ValueTask<StringArg?> InternalExecute(
        ISupportTextSearch context,
        StringArg arg,
        CancellationToken cancel
    )
    {
        context.Query(arg.Value);
        return ValueTask.FromResult<StringArg?>(null);
    }
}
