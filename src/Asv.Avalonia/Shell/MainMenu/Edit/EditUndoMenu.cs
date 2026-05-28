using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public class EditUndoMenu : MenuItem
{
    public const string MenuId = $"{EditMenu.MenuId}-undo";

    public EditUndoMenu(IShellHost shellHost, IHotKeyService hotKeys)
        : base(MenuId, RS.UndoCommand_CommandInfo_Name, EditMenu.MenuId)
    {
        shellHost.ExecuteNowOrWhenShellLoaded(InitShell).AddTo(ref DisposableBag);
        Icon = UndoAction.IconKind;
        BindHotKey(hotKeys, UndoAction.Id);
        Order = 0;
    }

    private void InitShell(IShell shell, TopLevel topLevel)
    {
        shell
            .SelectedPage.DistinctUntilChanged()
            .Subscribe(SelectedPageChanged)
            .AddTo(ref DisposableBag);
    }

    private void SelectedPageChanged(IPage? page)
    {
        Command = page?.UndoHistory.Undo;
        IsEnabled = Command is not null;
    }
}
