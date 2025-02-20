using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IConnectionPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
    BindableReactiveProperty<bool> IsCompactMode { get; }
}

public interface IConnectionDialog: IRoutable, IExportable
{
    ValueTask Init(IConnectionPage context);
}