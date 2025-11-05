using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example;

public sealed class ControlsGalleryPageViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class ControlsGalleryPageViewModel
    : TreePageViewModel<
        IControlsGalleryPage,
        IControlsGallerySubPage,
        ControlsGalleryPageViewModelConfig
    >,
        IControlsGalleryPage
{
    public const string PageId = "controls_gallery";
    public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

    public ControlsGalleryPageViewModel()
        : this(
            DesignTime.CommandService,
            DesignTime.ContainerHost,
            DesignTime.Configuration,
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
        IConfiguration cfg,
        ILoggerFactory loggerFactory, 
        IDialogService dialogService
    )
        : base(PageId, cmd, containerHost, cfg, loggerFactory, dialogService)
    {
        Title = RS.ControlsGalleryPageViewModel_Title;
        Icon = PageIcon;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
