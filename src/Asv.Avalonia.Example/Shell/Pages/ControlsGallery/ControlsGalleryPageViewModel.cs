using System;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example;

public class ControlsGalleryPageViewModel
    : TreePageViewModel<IControlsGalleryPage, IControlsGallerySubPage>,
        IControlsGalleryPage
{
    public const string PageId = "controls_gallery";
    public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;
    public const AsvColorKind PageIconColor = AsvColorKind.Info20;

    public ControlsGalleryPageViewModel()
        : this(
            DesignTime.PageContext,
            DesignTime.CommandService,
            AppHost.Instance.Services,
            NullLayoutService.Instance,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ControlsGalleryPageViewModel(
        IPageContext context,
        ICommandService cmd,
        IServiceProvider containerHost,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, cmd, containerHost, layoutService, loggerFactory, dialogService, ext)
    {
        Title = RS.ControlsGalleryPageViewModel_Title;
        TreeHeader = RS.ControlsGalleryPageViewModel_TreeHeader;
        Icon = PageIcon;
        IconColor = PageIconColor;
    }

    public void ChangeStatus(MaterialIconKind? statusIcon, AsvColorKind color)
    {
        Status = statusIcon;
        StatusColor = color;
    }
}
