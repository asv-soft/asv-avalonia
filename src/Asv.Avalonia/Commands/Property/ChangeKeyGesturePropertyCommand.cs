using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeKeyGesturePropertyCommand : ContextCommand<IHistoricalProperty<KeyGesture?>>
{
    #region Static

    public const string Id = $"{BaseId}.property.keygesture";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeKeyGesturePropertyCommand_CommandInfo_Name,
        Description = RS.ChangeKeyGesturePropertyCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PropertyTag,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    protected override ValueTask<ICommandArg?> InternalExecute(
        IHistoricalProperty<KeyGesture?> context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not KeyGestureCommandArg value)
        {
            throw new InvalidCastException(
                $"Invalid value type. Persistable must be a {typeof(KeyGesture)}"
            );
        }

        var oldValue = new KeyGestureCommandArg(context.ModelValue.Value);
        context.ModelValue.OnNext(value.Value);
        return ValueTask.FromResult<ICommandArg?>(oldValue);
    }
}
