using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeDoublePropertyCommand : ContextCommand<IHistoricalProperty<double>>
{
    #region Static

    public const string Id = $"{BaseId}.property.double";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeDoublePropertyCommand_CommandInfo_Name,
        Description = RS.ChangeDoublePropertyCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PropertyTag,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    protected override ValueTask<ICommandArg?> InternalExecute(
        IHistoricalProperty<double> context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not DoubleCommandArg value)
        {
            throw new InvalidCastException("Invalid value type. Persistable must be double");
        }

        var oldValue = new DoubleCommandArg(context.ModelValue.Value);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<ICommandArg?>(oldValue);
    }
}
