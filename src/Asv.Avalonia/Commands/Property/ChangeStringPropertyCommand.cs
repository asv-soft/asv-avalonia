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

    protected override ValueTask<IPersistable?> InternalExecute(
        IHistoricalProperty<string?> context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not Persistable<string?> value)
        {
            throw new InvalidCastException("Invalid value type. Persistable must be a string");
        }

        var oldValue = new Persistable<string?>(context.ModelValue.Value);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<IPersistable?>(oldValue);
    }
}
