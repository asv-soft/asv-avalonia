using System.Composition;
using Asv.Cfg;
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
            NullLoggerFactory.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public ControlsGalleryPageViewModel(
        ICommandService cmd,
        IContainerHost containerHost,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cmd, containerHost, layoutService, loggerFactory)
    {
        Title = RS.ControlsGalleryPageViewModel_Title;
        Icon = PageIcon;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
