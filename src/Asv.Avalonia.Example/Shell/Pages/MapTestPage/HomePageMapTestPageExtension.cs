using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Example;

public class HomePageMapTestPageExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.map-test";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-map-test")
        {
            Header = RS.OpenMapTestPageCommand_Action_Name,
            Description = RS.OpenMapTestPageCommand_Action_Description,
            Icon = MapTestPageViewModel.PageIcon,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(MapTestPageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}
