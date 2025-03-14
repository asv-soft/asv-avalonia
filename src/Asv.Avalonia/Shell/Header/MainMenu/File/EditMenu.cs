using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Material.Icons;
using R3;

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
    public const string MenuId = "shell.menu.edit.undo";
    private readonly BindableAsyncCommand _undoCommand;

    [ImportingConstructor]
    public EditUndoMenu()
        : base(MenuId, RS.UndoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        _undoCommand = new BindableAsyncCommand(UndoCommand.Id, this);
        Command = _undoCommand;
        _undoCommand.CanExecuteChanged += (s, e) => IsEnabled = _undoCommand.CanExecute(null);
        IsEnabled = _undoCommand.CanExecute(null);
    }
}

[ExportMainMenu]
public class EditRedoMenu : MenuItem
{
    public const string MenuId = "shell.menu.edit.redo";
    private readonly BindableAsyncCommand _redoCommand;

    [ImportingConstructor]
    public EditRedoMenu()
        : base(MenuId, RS.RedoCommand_Name, EditMenu.MenuId)
    {
        _redoCommand = new BindableAsyncCommand(RedoCommand.Id, this);
        Command = _redoCommand;
        _redoCommand.CanExecuteChanged += (s, e) => IsEnabled = _redoCommand.CanExecute(null);
        IsEnabled = _redoCommand.CanExecute(null);
    }
}