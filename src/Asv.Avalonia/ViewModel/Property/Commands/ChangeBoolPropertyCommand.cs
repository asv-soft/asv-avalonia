using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeBoolPropertyCommand : ContextCommand<IHistoricalProperty<bool>>
{
    #region Static

    public const string Id = $"{BaseId}.property.bool";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeBoolPropertyCommand_CommandInfo_Name,
        Description = RS.ChangeBoolPropertyCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PropertyTag,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    protected override ValueTask<CommandArg?> InternalExecute(
        IHistoricalProperty<bool> context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not BoolArg value)
        {
            throw new CommandArgMismatchException(typeof(BoolArg));
        }

        var oldValue = new BoolArg(context.ModelValue.Value);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<CommandArg?>(oldValue);
    }
}
