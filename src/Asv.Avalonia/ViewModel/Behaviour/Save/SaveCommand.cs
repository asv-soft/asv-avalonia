using System.Composition;
using Material.Icons;

namespace Asv.Avalonia.Save;

[ExportCommand]
[Shared]
public class SaveCommand : ContextCommand<ISupportSave>
{
    public const string Id = $"{BaseId}.save";

    public override ICommandInfo Info =>
        new CommandInfo
        {
            Id = Id,
            Name = "Save",
            Description = "Save changes",
            Icon = MaterialIconKind.FloppyDisc,
            DefaultHotKey = "Ctrl+S", 
            Source = SystemModule.Instance,
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
