using Asv.Modeling;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class OpenHomePageAction : HotKeyAction<IViewModel>
{
    public override string Id => OpenHomePageCommand.Id;
    public override string Name => OpenHomePageCommand.StaticInfo.Name;
    public override string Description => OpenHomePageCommand.StaticInfo.Description;
    public override MaterialIconKind Icon => OpenHomePageCommand.StaticInfo.Icon;
    public override KeyGesture DefaultHotKey => new(Key.H, KeyModifiers.Control);

    protected override async ValueTask<bool> Execute(IViewModel target, CancellationToken cancel)
    {
        await target.GoTo(new NavPath(new NavId(HomePageViewModel.PageId)));
        return true;
    }
}
