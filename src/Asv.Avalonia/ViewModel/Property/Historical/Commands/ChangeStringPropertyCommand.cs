using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeStringPropertyCommand : ContextCommand<IHistoricalProperty<string?>>
{
    #region Static

    public const string Id = $"{BaseId}.property.string";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeStringPropertyCommand_CommandInfo_Name,
        Description = RS.ChangeStringPropertyCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PropertyTag,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    protected override ValueTask<CommandArg?> InternalExecute(
        IHistoricalProperty<string?> context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not StringArg value)
        {
            throw new CommandArgMismatchException(typeof(StringArg));
        }

        var oldValue = new StringArg(context.ModelValue.Value ?? string.Empty);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<CommandArg?>(oldValue);
    }
}
