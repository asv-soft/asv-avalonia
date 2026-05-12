using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class ToolsMenu : MenuItem
{
    public const string MenuId = "main-menu-tools";

    public ToolsMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Name)
    {
        Order = 50;
        Icon = MaterialIconKind.Tools;
    }
}
