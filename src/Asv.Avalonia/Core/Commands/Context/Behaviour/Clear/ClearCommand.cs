using Material.Icons;

namespace Asv.Avalonia;

public class ClearCommand : ContextCommand<ISupportClear>
{
    public const string Id = $"{BaseId}.clear";

    public override ICommandInfo Info =>
        new CommandInfo
        {
            Id = Id,
            Name = RS.ClearCommand_CommandInfo_Name,
            Description = RS.ClearCommand_CommandInfo_Description,
            Icon = MaterialIconKind.Clear,
            DefaultHotKey = "Ctrl+Escape", // TODO: fix hotkey
        };

    protected override ValueTask<CommandArg?> InternalExecute(
        ISupportClear context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        context.Clear();
        return CommandArg.Null;
    }
}
