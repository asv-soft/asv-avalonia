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
        if (newValue is null)
        {
            context.ConfirmChangeHotKeyImpl();
        }
        else
        {
            // context.CurrentHotKeyValue.Value = null;
        }

        return ValueTask.FromResult<ICommandArg?>(null);
    }
}
