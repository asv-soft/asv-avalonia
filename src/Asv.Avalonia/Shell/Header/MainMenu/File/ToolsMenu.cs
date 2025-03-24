namespace Asv.Avalonia;

[ExportMainMenu]
public class ToolsMenu : MenuItem
{
    public const string MenuId = "shell.menu.tools";

    public ToolsMenu()
        : base(MenuId, RS.ToolsMenu_Name) { }
}

[ExportMainMenu]
public class ToolsSettingsMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.settings";

    public ToolsSettingsMenu()
        : base(MenuId, RS.ToolsMenu_Settings, ToolsMenu.MenuId)
    {
        Command = new BindableAsyncCommand(OpenSettingsCommand.Id, this);
    }
}
