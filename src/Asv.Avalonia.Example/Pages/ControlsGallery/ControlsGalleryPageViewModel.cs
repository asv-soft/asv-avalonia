using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class ControlsGalleryPageViewModel
    : TreePageViewModel<IControlsGalleryPage, IControlsGallerySubPage>,
        IControlsGalleryPage
{
    public const string PageId = "controls_gallery";
    public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

    public ControlsGalleryPageViewModel()
        : this(
            DesignTime.CommandService,
            DesignTime.ContainerHost,
            NullLayoutService.Instance,
            NullLoggerFactory.Instance,
            DesignTime.DialogService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public ControlsGalleryPageViewModel(
        ICommandService cmd,
        IContainerHost containerHost,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, cmd, containerHost, layoutService, loggerFactory, dialogService)
    {
        Title = RS.ControlsGalleryPageViewModel_Title;
        Icon = PageIcon;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
