using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
[method: ImportingConstructor]
public class ToolsMenu(ILoggerFactory loggerFactory)
    : MenuItem(MenuId, RS.ToolsMenu_Name, loggerFactory)
{
    public const string MenuId = "shell.menu.tools";
}

[ExportMainMenu]
public class ToolsHomeMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.home";

    [ImportingConstructor]
    public ToolsHomeMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Home, loggerFactory, ToolsMenu.MenuId)
    {
        Command = new BindableAsyncCommand(OpenHomePageCommand.Id, this);
    }
}

[ExportMainMenu]
public class ToolsSettingsMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.settings";

    [ImportingConstructor]
    public ToolsSettingsMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Settings, loggerFactory, ToolsMenu.MenuId)
    {
        Command = new BindableAsyncCommand(OpenSettingsCommand.Id, this);
    }
}
