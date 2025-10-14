using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class EditMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.edit";

    [ImportingConstructor]
    public EditMenu(ILayoutService layoutService, ILoggerFactory loggerFactory)
        : base(MenuId, RS.ShellView_Toolbar_Edit, layoutService, loggerFactory)
    {
        Order = 0;
        Icon = MaterialIconKind.PencilBoxOutline;
    }
}
