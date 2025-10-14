using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class ToolsMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.tools";

    [method: ImportingConstructor]
    public ToolsMenu(ILayoutService layoutService, ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Name, layoutService, loggerFactory)
    {
        Order = 50;
        Icon = MaterialIconKind.Tools;
    }
}
