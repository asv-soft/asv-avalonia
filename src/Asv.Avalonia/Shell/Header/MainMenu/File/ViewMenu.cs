namespace Asv.Avalonia;

[ExportMainMenu]
public class ViewMenu : MenuItem
{
    public const string MenuId = "shell.menu.view";

    public ViewMenu()
        : base(MenuId, RS.ShellView_Toolbar_View) { }
}
