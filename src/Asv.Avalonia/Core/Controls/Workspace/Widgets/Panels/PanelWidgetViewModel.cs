using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class PanelWidgetViewModel : WorkspaceWidget
{
    protected PanelWidgetViewModel(NavId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        ItemsSource = new ObservableList<IViewModel>();
        ItemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        ItemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        ItemsView = ItemsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public ObservableList<IViewModel> ItemsSource { get; }
    public NotifyCollectionChangedSynchronizedViewList<IViewModel> ItemsView { get; }
}
