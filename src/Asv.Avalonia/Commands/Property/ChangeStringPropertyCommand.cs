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
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    protected override ValueTask<ICommandArg?> InternalExecute(
        IHistoricalProperty<string?> context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not StringCommandArg value)
        {
            throw new CommandArgMismatchException(typeof(StringCommandArg));
        }

        var oldValue = new StringCommandArg(context.ModelValue.Value ?? string.Empty);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<ICommandArg?>(oldValue);
    }
}
