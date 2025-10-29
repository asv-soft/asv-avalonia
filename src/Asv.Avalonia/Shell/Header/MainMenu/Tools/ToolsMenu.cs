using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class ToolsMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.tools";

    [method: ImportingConstructor]
    public ToolsMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ToolsMenu_Name, loggerFactory)
    {
        Order = 50;
        Icon = MaterialIconKind.Tools;
    }
}
