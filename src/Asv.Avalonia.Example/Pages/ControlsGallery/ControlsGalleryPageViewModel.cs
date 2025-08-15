using System.Collections.Generic;
using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example;

public interface IControlsGallerySubPage : ITreeSubpage<ControlsGalleryPageViewModel> { }

[ExportPage(PageId)]
public class ControlsGalleryPageViewModel
    : TreePageViewModel<ControlsGalleryPageViewModel, IControlsGallerySubPage>
{
    public const string PageId = "controls_gallery";
    public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

    public ControlsGalleryPageViewModel()
        : this(DesignTime.CommandService, DesignTime.ContainerHost, NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public ControlsGalleryPageViewModel(
        ICommandService cmd,
        IContainerHost containerHost,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cmd, containerHost, loggerFactory)
    {
        Title = RS.ControlsGalleryViewModel_Title;
        Icon = PageIcon;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
