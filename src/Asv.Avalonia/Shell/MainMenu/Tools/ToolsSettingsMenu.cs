using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class ToolsSettingsMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.settings";

    public ToolsSettingsMenu()
        : base(MenuId, RS.ToolsMenu_Settings, ToolsMenu.MenuId)
    {
        Icon = MaterialIconKind.Cog;
        Command = new ReactiveCommand(_ =>
            this.GoTo(new NavPath(new NavId(SettingsPageViewModel.PageId)))
        ).DisposeItWith(Disposable);
    }
}
