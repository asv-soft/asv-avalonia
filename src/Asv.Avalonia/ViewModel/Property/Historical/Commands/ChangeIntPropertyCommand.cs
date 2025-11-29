using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeIntPropertyCommand : ContextCommand<IHistoricalProperty<int>, IntArg>
{
    #region Static

    public const string Id = $"{BaseId}.property.int";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeIntPropertyCommand_CommandInfo_Name,
        Description = RS.ChangeIntPropertyCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PropertyTag,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    public override ValueTask<IntArg?> InternalExecute(
        IHistoricalProperty<int> context,
        IntArg arg,
        CancellationToken cancel
    )
    {
        var oldValue = new IntArg(context.ModelValue.Value);
        context.ModelValue.OnNext((int)arg.Value);
        return ValueTask.FromResult<IntArg?>(oldValue);
    }
}
