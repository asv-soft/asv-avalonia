using System.Composition;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IModelProperty : IRoutable
{
    ReactiveProperty<double> ModelValue { get; }
}

[ExportCommand]
[Shared]
public class ChangeDoublePropertyCommand : ContextCommand<IModelProperty>
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
        IModelProperty context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not Persistable<double> value)
        {
            throw new Exception("Invalid value type. Persistable must be double");
        }

        var oldValue = new Persistable<double>(context.ModelValue.Value);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<IPersistable?>(oldValue);
    }
}
