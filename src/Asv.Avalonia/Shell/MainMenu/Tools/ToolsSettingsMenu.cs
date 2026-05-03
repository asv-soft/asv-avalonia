using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class ToolsSettingsMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.settings";

    public ToolsSettingsMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Settings, ToolsMenu.MenuId)
    {
        Icon = OpenSettingsCommand.StaticInfo.Icon;
        Command = new BindableAsyncCommand(OpenSettingsCommand.Id, this);
    }
}
