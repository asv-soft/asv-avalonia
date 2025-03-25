using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class UndoCommand : ContextCommand<IShell>
{
    public const string Id = $"{BaseId}.global.undo";

    public static ICommandInfo StaticInfo { get; } =
        new CommandInfo
        {
            Id = Id,
            Name = RS.UndoCommand_CommandInfo_Name,
            Description = RS.UndoCommand_CommandInfo_Description,
            Icon = MaterialIconKind.UndoVariant,
            DefaultHotKey = KeyGesture.Parse("Ctrl+Z"),
            Source = SystemModule.Instance,
        };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandParameter?> InternalExecute(
        IShell context,
        ICommandParameter newValue,
        CancellationToken cancel
    )
    {
        var selectedPage = context.SelectedPage.CurrentValue;
        if (selectedPage?.History.Undo.CanExecute() == true)
        {
            await selectedPage.History.UndoAsync(cancel);
        }

        return null;
    }
}
