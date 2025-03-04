using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeDoublePropertyCommand : ContextCommand<IHistoricalProperty<double?>>
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

    protected override ValueTask<IPersistable?> InternalExecute(
        IHistoricalProperty<double?> context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not Persistable<double> value)
        {
            throw new InvalidCastException("Invalid value type. Persistable must be double");
        }

        var oldValue = new Persistable<double?>(context.ModelValue.Value);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<IPersistable?>(oldValue);
    }
}
