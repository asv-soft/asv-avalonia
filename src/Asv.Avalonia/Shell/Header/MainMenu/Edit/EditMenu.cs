using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class EditMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.edit";

    public EditMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ShellView_Toolbar_Edit, loggerFactory)
    {
        Order = 0;
        Icon = MaterialIconKind.PencilBoxOutline;
    }
}
