namespace Asv.Avalonia;

public class EditUndoMenu : MenuItem
{
    public const string MenuId = $"{EditMenu.MenuId}-undo";

    public EditUndoMenu(IShellHost shellHost, IHotKeyService hotKeys)
        : base(MenuId, RS.UndoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        this.BindHistoryCommand(shellHost, page => page.UndoHistory.Undo, Disposable);
        Icon = UndoAction.IconKind;
        BindHotKey(hotKeys, UndoAction.Id, true);
        Order = 0;
    }
}
