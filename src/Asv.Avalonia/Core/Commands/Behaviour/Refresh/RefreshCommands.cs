using Material.Icons;

namespace Asv.Avalonia;

public class RefreshCommand : ContextCommand<ISupportRefresh>
{
    public const string Id = $"{BaseId}.refresh";

    public override ICommandInfo Info =>
        new CommandInfo
        {
            Id = Id,
            Name = RS.RefreshCommand_CommandInfo_Name,
            Description = RS.RefreshCommand_CommandInfo_Description,
            Icon = MaterialIconKind.Refresh,
            DefaultHotKey = "F5",
        };

    protected override ValueTask<CommandArg?> InternalExecute(
        ISupportRefresh context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        context.Refresh();
        return CommandArg.Null;
    }
}
