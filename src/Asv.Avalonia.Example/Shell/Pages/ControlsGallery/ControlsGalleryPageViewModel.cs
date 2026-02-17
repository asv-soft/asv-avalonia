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
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ControlsGalleryPageViewModel(
        ICommandService cmd,
        IContainerHost containerHost,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, cmd, containerHost, layoutService, loggerFactory, dialogService, ext)
    {
        Title = RS.ControlsGalleryPageViewModel_Title;
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
