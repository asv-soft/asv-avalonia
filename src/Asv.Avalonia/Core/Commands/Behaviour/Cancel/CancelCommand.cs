using Material.Icons;

namespace Asv.Avalonia;

public class CancelCommand : ContextCommand<ISupportCancel>
{
    public const string Id = $"{BaseId}.cancel";

    public override ICommandInfo Info =>
        new CommandInfo
        {
            Id = Id,
            Name = RS.CancelCommand_CommandInfo_Name,
            Description = RS.CancelCommand_CommandInfo_Description,
            Icon = MaterialIconKind.Cancel,
            DefaultHotKey = "Ctrl+F5", // TODO: fix hotkey
        };

    protected override ValueTask<CommandArg?> InternalExecute(
        ISupportCancel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        context.Cancel();
        return CommandArg.Null;
    }
}
