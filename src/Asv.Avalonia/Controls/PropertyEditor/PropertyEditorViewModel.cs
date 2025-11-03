using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class PropertyEditorViewModel : RoutableViewModel
{
    private readonly ObservableList<IPropertyViewModel> _itemsSource;

    public PropertyEditorViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        _itemsSource.Add(new UnitPropertyViewModel());
        _itemsSource.Add(new UnitPropertyViewModel());
        _itemsSource.Add(new UnitPropertyViewModel());
        Parent = DesignTime.Shell;
    }

    public PropertyEditorViewModel(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        _itemsSource = new ObservableList<IPropertyViewModel>();
        _itemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        Items = _itemsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Disposable.AddAction(() => _itemsSource.ClearWithItemsDispose());
    }

    public ObservableList<IPropertyViewModel> ItemsSource => _itemsSource;

    public NotifyCollectionChangedSynchronizedViewList<IPropertyViewModel> Items { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return _itemsSource;
    }
}
