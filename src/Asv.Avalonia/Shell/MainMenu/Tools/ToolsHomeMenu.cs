using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class ToolsHomeMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.home";

    public ToolsHomeMenu(IHotKeyService hotKeys)
        : base(MenuId, RS.ToolsMenu_Home, ToolsMenu.MenuId)
    {
        Icon = MaterialIconKind.Home;
        HotKey = hotKeys[OpenHomePageAction.Id];
        Command = new ReactiveCommand(_ =>
            this.GoTo(new NavPath(new NavId(HomePageViewModel.PageId)))
        ).DisposeItWith(Disposable);
    }
}
