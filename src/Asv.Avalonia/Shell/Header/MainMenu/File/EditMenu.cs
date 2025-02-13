namespace Asv.Avalonia;

[ExportMainMenu]
public class EditMenu : MenuItem
{
    public const string MenuId = "shell.menu.edit";

    public EditMenu()
        : base(MenuId, RS.ShellView_Toolbar_Edit) { }
}
