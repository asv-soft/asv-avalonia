using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class ViewMenu : MenuItem
{
    public const string MenuId = $"{MainMenuDefaultMenuExtender.Contract}.view";

    public ViewMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ShellView_Toolbar_View, loggerFactory)
    {
        Order = 80;
        Icon = MaterialIconKind.ViewGrid;
    }
}
