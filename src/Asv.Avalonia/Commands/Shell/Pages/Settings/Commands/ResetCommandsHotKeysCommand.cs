using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ResetCommandHotKeysCommand : ContextCommand<SettingsCommandListViewModel, DictArg>
{
    public const string Id = $"{BaseId}.settings.hotkeys.reset";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ResetCommandHotKeysCommand_CommandInfo_Name,
        Description = RS.ResetCommandHotKeysCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Refresh,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<DictArg?> InternalExecute(
        SettingsCommandListViewModel context,
        DictArg arg,
        CancellationToken cancel
    )
    {
        var previousChangedHotKeys = new DictArg();

        foreach (var commandViewModel in context.Items)
        {
            var currentCommandHotKey = commandViewModel.EditedHotKey.ModelValue.CurrentValue;

            if (currentCommandHotKey != CommandViewModel.EmptyHotKey)
            {
                previousChangedHotKeys[commandViewModel.CommandId] = CommandArg.CreateString(
                    currentCommandHotKey ?? string.Empty
                );
            }

            if (arg.TryGetValue(commandViewModel.CommandId, out var hotkeyToSetRaw))
            {
                commandViewModel.SetHotKey(hotkeyToSetRaw.AsString());
            }
        }

        return previousChangedHotKeys.Count > 0
            ? ValueTask.FromResult<DictArg?>(previousChangedHotKeys)
            : ValueTask.FromResult<DictArg?>(null);
    }
}
