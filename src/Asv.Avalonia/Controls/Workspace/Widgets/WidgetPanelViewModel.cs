using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class WidgetPanelViewModel : WorkspaceWidget
{
    public WidgetPanelViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        ItemsSource.Add(new SingleRttBoxViewModel());
        ItemsSource.Add(new SingleRttBoxViewModel());
        ItemsSource.Add(new SingleRttBoxViewModel());
        ItemsSource.Add(new KeyValueRttBoxViewModel());
        ItemsSource.Add(new KeyValueRttBoxViewModel());
        ItemsSource.Add(new SplitDigitRttBoxViewModel());
        ItemsSource.Add(new SplitDigitRttBoxViewModel());
        ItemsSource.Add(new TwoColumnRttBoxViewModel());
        ItemsSource.Add(new TwoColumnRttBoxViewModel());
        ItemsSource.Add(new GeoPointRttBoxViewModel());
        ItemsSource.Add(new GeoPointRttBoxViewModel());
    }

    public WidgetPanelViewModel(NavigationId id, ILoggerFactory loggerFactory)
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
