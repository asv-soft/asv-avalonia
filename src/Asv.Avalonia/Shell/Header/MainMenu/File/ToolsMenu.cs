namespace Asv.Avalonia;

[ExportMainMenu]
public class ToolsMenu : MenuItem
{
    public const string MenuId = "shell.menu.tools";

    public ToolsMenu()
        : base(MenuId, "Tools") // TODO: Localize
    { }
}

[ExportMainMenu]
public class ToolsSettingsMenu : MenuItem
{
    public const string MenuId = "shell.menu.tools.settings";

    public ToolsSettingsMenu()
        : base(MenuId, "Settings", ToolsMenu.MenuId) // TODO: Localize
    {
        Command = new BindableAsyncCommand(OpenSettingsCommand.Id, this);
    }
}
