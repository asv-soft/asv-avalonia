using Material.Icons;

namespace Asv.Avalonia;

public class ViewMenu : MenuItem
{
    public const string MenuId = $"{MainMenuDefaultMenuExtender.Contract}.view";

    public ViewMenu()
        : base(MenuId, RS.ShellView_Toolbar_View)
    {
        Order = 80;
        Icon = MaterialIconKind.ViewGrid;
    }
}
