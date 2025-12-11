using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class PanelWidgetViewModel : WorkspaceWidget
{
    protected PanelWidgetViewModel(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        ItemsSource = new ObservableList<IRoutable>();
        ItemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        ItemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        ItemsView = ItemsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public ObservableList<IRoutable> ItemsSource { get; }
    public NotifyCollectionChangedSynchronizedViewList<IRoutable> ItemsView { get; }
}
