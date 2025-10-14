using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia.Example;

public interface IControlsGalleryPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
}

public interface IControlsGallerySubPage : ITreeSubpage<IControlsGalleryPage> { }

public abstract class ControlsGallerySubPage(
    NavigationId id,
    ILayoutService layoutService,
    ILoggerFactory loggerFactory
) : TreeSubpage<IControlsGalleryPage>(id, layoutService, loggerFactory), IControlsGallerySubPage
{
    public override ValueTask Init(IControlsGalleryPage context) => ValueTask.CompletedTask;

    public override IEnumerable<IRoutable> GetRoutableChildren() => Menu;
}
