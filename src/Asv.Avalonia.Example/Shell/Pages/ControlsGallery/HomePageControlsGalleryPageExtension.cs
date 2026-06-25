using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Example;

public class HomePageControlsGalleryPageExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.controls-gallery";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-controls-gallery")
        {
            Header = RS.OpenControlsGalleryPageCommand_Action_Title,
            Description = RS.OpenControlsGalleryPageCommand_Action_Description,
            Icon = ControlsGalleryPageViewModel.PageIcon,
            IconColor = ControlsGalleryPageViewModel.PageIconColor,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(ControlsGalleryPageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}
