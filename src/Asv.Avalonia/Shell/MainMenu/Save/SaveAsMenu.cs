using R3;

namespace Asv.Avalonia;

public class SaveAsMenu : MenuItem
{
    public const string MenuId = "main-menu-save-as";

    public SaveAsMenu(
        IHotKeyService hotKeys,
        IEnumerable<IHotKeyAction> actions,
        IShellHost shellHost
    )
        : base(MenuId, RS.SaveAsCommand_CommandInfo_Name)
    {
        var saveAsAction = actions.First(x => x.ActionId == SaveAsAction.Id);

        Order = -60;
        Icon = saveAsAction.Icon;
        BindHotKey(hotKeys, SaveAsAction.Id);
        Command = new ReactiveCommand(
            async (_, cancel) =>
            {
                var selected = shellHost.Shell?.Navigation.SelectedControl.CurrentValue;
                if (selected != null)
                {
                    await saveAsAction.Execute(selected, cancel);
                }
            }
        ).AddTo(ref DisposableBag);
    }
}
