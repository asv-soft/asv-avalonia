using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class EditMenu : MenuItem
{
    public const string MenuId = "main-menu-edit";

    public EditMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ShellView_Toolbar_Edit)
    {
        Order = 0;
        Icon = MaterialIconKind.PencilBoxOutline;
    }
}
