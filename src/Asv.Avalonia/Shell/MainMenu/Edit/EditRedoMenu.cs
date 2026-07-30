namespace Asv.Avalonia;

public class EditRedoMenu : MenuItem
{
    public const string MenuId = $"{EditMenu.MenuId}-redo";

    public EditRedoMenu(IShellHost shellHost, IHotKeyService hotKeys)
        : base(MenuId, RS.RedoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        this.BindHistoryCommand(shellHost, page => page.UndoHistory.Redo, Disposable);
        Icon = RedoAction.IconKind;
        BindHotKey(hotKeys, RedoAction.Id, true);
        Order = 1;
    }
}
