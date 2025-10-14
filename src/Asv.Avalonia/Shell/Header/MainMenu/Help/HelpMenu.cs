using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class HelpMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.help";

    [method: ImportingConstructor]
    public HelpMenu(ILayoutService layoutService, ILoggerFactory loggerFactory)
        : base(MenuId, RS.ShellView_Toolbar_Help, layoutService, loggerFactory)
    {
        Order = 100;
        Icon = MaterialIconKind.Help;
    }
}
