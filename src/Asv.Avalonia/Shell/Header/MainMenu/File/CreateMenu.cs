using System.Composition;
using System.Reactive.Disposables;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportMainMenu]
public class CreateMenu : MenuItem
{
    public const string MenuId = $"{ExportMainMenuAttribute.Contract}.create";

    [ImportingConstructor]
    public CreateMenu(ILayoutService layoutService, ILoggerFactory loggerFactory)
        : base(MenuId, "Create", layoutService, loggerFactory)
    {
        Order = -90;
        Icon = MaterialIconKind.FilePlus;
        Header = "Create";
    }
}
