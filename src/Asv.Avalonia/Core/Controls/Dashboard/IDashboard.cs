using ObservableCollections;

namespace Asv.Avalonia;

public interface IDashboard : IViewModel
{
    ObservableList<ITileViewModel> Tiles { get; }
    NotifyCollectionChangedSynchronizedViewList<ITileViewModel> Regular { get; }
    NotifyCollectionChangedSynchronizedViewList<ITileViewModel> Compact { get; }
    NotifyCollectionChangedSynchronizedViewList<ITileViewModel> Inline { get; }
}
