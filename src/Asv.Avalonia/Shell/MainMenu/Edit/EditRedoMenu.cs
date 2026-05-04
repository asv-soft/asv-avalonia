using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public class EditRedoMenu : MenuItem
{
    public const string MenuId = "redo";

    public EditRedoMenu(IShellHost shellHost)
        : base(MenuId, RS.RedoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        shellHost.ExecuteNowOrWhenShellLoaded(InitShell).AddTo(ref DisposableBag);
        Icon = RedoAction.StaticInfo.Icon;
        Order = 1;
    }

    private void InitShell(IShell shell, TopLevel topLevel)
    {
        shell.SelectedPage.DistinctUntilChanged().Subscribe(SelectedPageChanged).AddTo(ref DisposableBag);
    }

    private void SelectedPageChanged(IPage? page)
    {
        if (page == null)
        {
            Command = null;
            IsEnabled = false;
            return;
        }

        Command = page.UndoHistory.Undo;
    }
}
