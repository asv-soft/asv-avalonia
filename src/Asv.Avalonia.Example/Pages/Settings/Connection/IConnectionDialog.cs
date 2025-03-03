using System.Threading.Tasks;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public interface IConnectionPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
    BindableReactiveProperty<bool> IsCompactMode { get; }
}

public interface IConnectionDialog : IRoutable, IExportable
{
    ValueTask Init(IConnectionPage context);
}
