using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public sealed class ViewSaveMenu : MenuItem
{
    public const string MenuId = $"{ViewMenu.MenuId}-save";

    private readonly IShellHost _shellHost;
    private readonly ILayoutService _layoutService;

    public ViewSaveMenu(IShellHost shellHost, ILayoutService layoutService)
        : base(MenuId, RS.ViewSaveMenu_Header, ViewMenu.MenuId)
    {
        _shellHost = shellHost;
        _layoutService = layoutService;

        Order = 0;
        Icon = MaterialIconKind.ContentSave;
        Command = new ReactiveCommand(SaveCurrentLayoutAsync).DisposeItWith(Disposable);
    }

    private async ValueTask SaveCurrentLayoutAsync(Unit unit, CancellationToken cancel)
    {
        var page = _shellHost.Shell?.SelectedPage.Value;
        if (page is not null)
        {
            await page.RequestSaveLayoutToFile(_layoutService, cancel);
        }
    }
}
