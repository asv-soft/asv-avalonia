using System.Collections.Generic;
using System.Threading.Tasks;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia.Example;

public interface IControlsGalleryPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
    void ChangeStatus(MaterialIconKind? statusIcon, AsvColorKind color = AsvColorKind.None);
}

public interface IControlsGallerySubPage : ITreeSubpage<IControlsGalleryPage> { }

public abstract class ControlsGallerySubPage(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage<IControlsGalleryPage>(id, loggerFactory),
        IControlsGallerySubPage
{
    public override ValueTask Init(IControlsGalleryPage context) => ValueTask.CompletedTask;

    public override IEnumerable<IRoutable> GetChildren() => Menu;
}
