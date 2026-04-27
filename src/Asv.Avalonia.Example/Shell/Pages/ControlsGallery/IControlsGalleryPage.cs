using System.Collections.Generic;
using System.Threading.Tasks;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia.Example;

public interface IControlsGalleryPage : ITreePageViewModel
{
    void ChangeStatus(MaterialIconKind? statusIcon, AsvColorKind color = AsvColorKind.None);
}

public interface IControlsGallerySubPage : ITreeSubpage { }

public abstract class ControlsGallerySubPage(string id, ITreeSubPageContext<IControlsGalleryPage> context)
    : TreeSubpage<IControlsGalleryPage>(id, context),
        IControlsGallerySubPage
{
    public override IEnumerable<IViewModel> GetChildren() => Menu;
}
