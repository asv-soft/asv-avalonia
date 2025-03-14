using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class RedoCommand : ContextCommand<IShell>
{
    public const string Id = $"{BaseId}.global.redo";

    public static ICommandInfo StaticInfo { get; } = new CommandInfo
    {
        Id = Id,
        Name = RS.RedoCommand_Name,
        Description = RS.RedoCommand_CommandInfo_Description,
        Icon = MaterialIconKind.RedoVariant,
        DefaultHotKey = KeyGesture.Parse("Ctrl+Y"),
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<IPersistable?> InternalExecute(
        IShell context,
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        var selectedPage = context.SelectedPage.Value;
        if (selectedPage != null && selectedPage.History.Redo.CanExecute())
        {
            await selectedPage.History.RedoAsync(cancel);
        }
        
        return null;
    }

    public bool CanExecute(IShell context)
    {
        var selectedPage = context.SelectedPage.Value;
        return selectedPage?.History.Redo.CanExecute() ?? false;
    }
}