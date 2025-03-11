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
    private readonly IShellHost _shell;
    private readonly ReactiveCommand _undoCommand;

    [ImportingConstructor]
    public EditUndoMenu(IShellHost shell)
        : base(MenuId, RS.UndoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        _shell = shell;
        _undoCommand = new ReactiveCommand(async (_, token) =>
        {
            var selectedPage = _shell.Shell.SelectedPage.Value;
            if (selectedPage != null)
            {
                var history = selectedPage.History;
                if (history.Undo.CanExecute())
                {
                    await Task.Run(() => history.Undo.Execute(default), token);
                }
            }
        });
        Command = _undoCommand;
        _undoCommand.CanExecuteChanged += (s, e) => OnPropertyChanged(nameof(IsEnabled));
    }

    public bool IsEnabled => _undoCommand.CanExecute();
}

[ExportMainMenu]
public class EditRedoMenu : MenuItem
{
    public const string MenuId = "shell.menu.edit.redo";
    private readonly IShellHost _shell;
    private readonly ReactiveCommand _redoCommand;

    [ImportingConstructor]
    public EditRedoMenu(IShellHost shell)
        : base(MenuId, RS.RedoCommand_Name, EditMenu.MenuId)
    {
        _shell = shell;
        _redoCommand = new ReactiveCommand(async (_, token) =>
        {
            var selectedPage = _shell.Shell.SelectedPage.Value;
            if (selectedPage != null)
            {
                var history = selectedPage.History;
                if (history.Redo.CanExecute())
                {
                    await Task.Run(() => history.Redo.Execute(default), token);
                }
            }
        });
        Command = _redoCommand;
        _redoCommand.CanExecuteChanged += (s, e) => OnPropertyChanged(nameof(IsEnabled));
    }

    public bool IsEnabled => _redoCommand.CanExecute();
}