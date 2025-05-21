using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class RedoCommand : ContextCommand<IShell>
{
    public const string Id = $"{BaseId}.global.redo";

    public static ICommandInfo StaticInfo { get; } =
        new CommandInfo
        {
            Id = Id,
            Name = RS.RedoCommand_CommandInfo_Name,
            Description = RS.RedoCommand_CommandInfo_Description,
            Icon = MaterialIconKind.RedoVariant,
            HotKeyInfo = new HotKeyInfo { DefaultHotKey = KeyGesture.Parse("Ctrl+Y") },
            Source = SystemModule.Instance,
        };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandArg?> InternalExecute(
        IShell context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var selectedPage = context.SelectedPage.CurrentValue;
        if (selectedPage?.History.Redo.CanExecute() == true)
        {
            await selectedPage.History.RedoAsync(cancel);
        }

        return null;
    }
}
