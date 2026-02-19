using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class HelpMenu : MenuItem
{
    public const string MenuId = $"{MainMenuDefaultMenuExtender.Contract}.help";

    public HelpMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.ShellView_Toolbar_Help, loggerFactory)
    {
        Order = 100;
        Icon = MaterialIconKind.Help;
    }
}
