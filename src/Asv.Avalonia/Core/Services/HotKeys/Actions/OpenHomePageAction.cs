using Asv.Modeling;
using Avalonia.Input;
using Material.Icons;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class OpenHomePageAction(IOptions<HomePageOptions> options) : HotKeyAction<IViewModel>
{
    public const string Id = "open.home";

    public override string ActionId => Id;
    public override string Name => RS.OpenHomePageCommand_CommandInfo_Name;
    public override string Description => RS.OpenHomePageCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => HomePageViewModel.PageIcon;
    public override KeyGesture DefaultHotKey => new(Key.H, KeyModifiers.Control);

    protected override async ValueTask InternalExecute(IViewModel target, CancellationToken cancel)
    {
        await target.GoTo(new NavPath(new NavId(options.Value.PageId)), cancel);
    }
}
