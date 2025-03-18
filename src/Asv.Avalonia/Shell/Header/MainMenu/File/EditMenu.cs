using System.Composition;

namespace Asv.Avalonia;

[ExportMainMenu]
public class EditMenu : MenuItem
{
    public const string MenuId = "shell.menu.edit";

    public EditMenu()
        : base(MenuId, RS.ShellView_Toolbar_Edit) { }
}

[ExportMainMenu]
public class EditUndoMenu : MenuItem
{
    public const string MenuId = $"{EditMenu.MenuId}.undo";

    [ImportingConstructor]
    public EditUndoMenu(IShellHost host)
        : base(MenuId, RS.UndoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        Command = new BindableAsyncCommand(UndoCommand.Id, this);
    }
}

[ExportMainMenu]
public class EditRedoMenu : MenuItem
{
    public const string MenuId = $"{EditMenu.MenuId}.redo";

    [ImportingConstructor]
    public EditRedoMenu()
        : base(MenuId, RS.RedoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        Command = new BindableAsyncCommand(RedoCommand.Id, this);
    }
}
