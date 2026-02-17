using Material.Icons;

namespace Asv.Avalonia.Save;

public class SaveCommand : ContextCommand<ISupportSave>
{
    public const string Id = $"{BaseId}.save";

    public override ICommandInfo Info =>
        new CommandInfo
        {
            Id = Id,
            Name = RS.SaveCommand_CommandInfo_Name,
            Description = RS.SaveCommand_CommandInfo_Description,
            Icon = MaterialIconKind.FloppyDisc,
            DefaultHotKey = "Ctrl+S",
        };

    protected override ValueTask<CommandArg?> InternalExecute(
        ISupportSave context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        context.Save();
        return CommandArg.Null;
    }
}
