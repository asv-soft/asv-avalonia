namespace Asv.Avalonia;

[ExportMainMenu]
public class HelpMenu : MenuItem
{
    public const string MenuId = "shell.menu.help";

    public HelpMenu()
        : base(MenuId, RS.ShellView_Toolbar_Help) { }
}
