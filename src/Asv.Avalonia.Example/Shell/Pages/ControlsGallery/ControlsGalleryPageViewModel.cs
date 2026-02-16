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
    public const AsvColorKind PageIconColor = AsvColorKind.Info20;

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
        TreeHeader = RS.ControlsGalleryPageViewModel_TreeHeader;
        Icon = PageIcon;
        IconColor = PageIconColor;
    }

    public override IExportInfo Source => SystemModule.Instance;

    public void ChangeStatus(MaterialIconKind? statusIcon, AsvColorKind color)
    {
        Status = statusIcon;
        StatusColor = color;
    }
}
