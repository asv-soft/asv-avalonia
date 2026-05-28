using R3;

namespace Asv.Avalonia;

public class SaveMenu : MenuItem
{
    public const string MenuId = "main-menu-save";

    public SaveMenu(
        IHotKeyService hotKeys,
        IEnumerable<IHotKeyAction> actions,
        IShellHost shellHost
    )
        : base(MenuId, RS.SaveCommand_CommandInfo_Name)
    {
        var saveAction = actions.First(x => x.ActionId == SaveAction.Id);

        Order = 1;
        Icon = saveAction.Icon;
        BindHotKey(hotKeys, SaveAction.Id);
        Command = new ReactiveCommand(
            async (_, cancel) =>
            {
                var selected = shellHost.Shell?.Navigation.SelectedControl.CurrentValue;
                if (selected != null)
                {
                    await saveAction.Execute(selected, cancel);
                }
            }
        ).AddTo(ref DisposableBag);
    }
}
