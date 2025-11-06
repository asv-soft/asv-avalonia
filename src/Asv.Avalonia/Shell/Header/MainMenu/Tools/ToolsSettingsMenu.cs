using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class ToolsSettingsMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.settings";

    [ImportingConstructor]
    public ToolsSettingsMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Settings, loggerFactory, ToolsMenu.MenuId)
    {
        Icon = OpenSettingsCommand.StaticInfo.Icon;
        Command = new BindableAsyncCommand(OpenSettingsCommand.Id, this);
    }
}
