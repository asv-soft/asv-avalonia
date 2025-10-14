using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class ToolsHomeMenu : MenuItem
{
    public const string MenuId = $"{ToolsMenu.MenuId}.home";

    [ImportingConstructor]
    public ToolsHomeMenu(ILayoutService layoutService, ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Home, layoutService, loggerFactory, ToolsMenu.MenuId)
    {
        Icon = OpenHomePageCommand.StaticInfo.Icon;
        Command = new BindableAsyncCommand(OpenHomePageCommand.Id, this);
    }
}
