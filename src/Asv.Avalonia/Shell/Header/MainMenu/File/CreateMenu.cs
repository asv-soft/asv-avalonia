using System.Reactive.Disposables;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class CreateMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.create";

    public CreateMenu(ILoggerFactory loggerFactory)
        : base(MenuId, RS.CreateMenu_Header, loggerFactory)
    {
        Order = -90;
        Icon = MaterialIconKind.FilePlus;
        Header = RS.CreateMenu_Header;
    }
}
