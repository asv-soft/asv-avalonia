using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class PanelWidgetViewModel : WorkspaceWidget
{
    protected PanelWidgetViewModel(string id, ILoggerFactory loggerFactory)
        : base(id)
    {
        ItemsSource = new ObservableList<IViewModel>();
        ItemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        ItemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        ItemsView = ItemsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public ObservableList<IViewModel> ItemsSource { get; }
    public NotifyCollectionChangedSynchronizedViewList<IViewModel> ItemsView { get; }
}
