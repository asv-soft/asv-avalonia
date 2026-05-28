using Material.Icons;

namespace Asv.Avalonia;

public class OpenMenu : MenuItem
{
    public const string MenuId = "main-menu-open";

    public OpenMenu()
        : base(MenuId, RS.ShellView_Toolbar_Open)
    {
        Order = -80;
        Icon = MaterialIconKind.FileOutline;
    }
}
