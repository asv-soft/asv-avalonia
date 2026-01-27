using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeDoublePropertyCommand : ContextCommand<IHistoricalProperty<double>>
{
    #region Static

    public const string Id = $"{BaseId}.property.double";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
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

    protected override ValueTask<CommandArg?> InternalExecute(
        IHistoricalProperty<double> context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not DoubleArg value)
        {
            throw new CommandArgMismatchException(typeof(DoubleArg));
        }

        var oldValue = new DoubleArg(context.ModelValue.Value);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<CommandArg?>(oldValue);
    }
}
