using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public sealed class ConfirmChangeHotKeyCommand : ContextCommand<SettingsCommandListItemViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.settings.commandlist.item.hotkey.confirm";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Confirm change hotkey",
        Description = "Command that confirms hotkey change",
        Icon = MaterialIconKind.KeyboardCaps,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        SettingsCommandListItemViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var oldValue = new KeyGestureCommandArg(context.Info.HotKeyInfo.CustomHotKey.Value);
        if (newValue == CommandArg.Empty)
        {
            context.ConfirmChangeHotKeyImpl();
        }
        else if (newValue is KeyGestureCommandArg arg)
        {
            context.CurrentHotKeyString.Value = arg.Value?.ToString();
            context.CurrentHotKey.Value = arg.Value;
            context.ConfirmChangeHotKeyImpl();
        }

        return ValueTask.FromResult<ICommandArg?>(oldValue);
    }
}
