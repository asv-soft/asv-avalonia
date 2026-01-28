using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeEnumPropertyCommand : ContextCommand<IHistoricalProperty<Enum>>
{
    #region Static

    public const string Id = $"{BaseId}.property.enum";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeEnumPropertyCommand_CommandInfo_Name,
        Description = RS.ChangeEnumPropertyCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PropertyTag,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    protected override ValueTask<CommandArg?> InternalExecute(
        IHistoricalProperty<Enum> context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not StringArg value)
        {
            throw new CommandArgMismatchException(typeof(StringArg));
        }

        var enumType = context.ModelValue.Value.GetType();

        var oldValue = new StringArg(
            Enum.GetName(enumType, context.ModelValue.Value) ?? string.Empty
        );

        if (
            !Enum.TryParse(enumType, value.Value, out var parsedEnum)
            || parsedEnum is not Enum newEnum
        )
        {
            throw new CommandException(
                $"{value.Value} is not a valid enum type for {nameof(enumType)}"
            );
        }

        context.ModelValue.OnNext(newEnum);

        return ValueTask.FromResult<CommandArg?>(oldValue);
    }
}
