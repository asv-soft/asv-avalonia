using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public sealed class ViewSaveAllMenu : MenuItem
{
    public const string MenuId = $"{ViewSaveMenu.MenuId}-all";

    private readonly ILayoutService _layoutService;

    public ViewSaveAllMenu(ILayoutService layoutService)
        : base(MenuId, RS.ViewSaveAllMenu_Header, ViewMenu.MenuId)
    {
        _layoutService = layoutService;

        Order = 1;
        Icon = MaterialIconKind.ContentSaveAll;
        Command = new ReactiveCommand(SaveAllLayoutAsync).DisposeItWith(Disposable);
    }

    private async ValueTask SaveAllLayoutAsync(Unit unit, CancellationToken cancel)
    {
        await this.RequestSaveAllLayoutToFile(_layoutService, cancel);
    }
}
