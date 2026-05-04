using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public class PropertyEditorViewModel : ViewModel
{
    private readonly ObservableList<IPropertyViewModel> _itemsSource;

    public PropertyEditorViewModel()
        : this(DesignTime.Id.TypeId)
    {
        DesignTime.ThrowIfNotDesignMode();
        _itemsSource.Add(new UnitPropertyViewModel());
        _itemsSource.Add(new UnitPropertyViewModel());
        _itemsSource.Add(new UnitPropertyViewModel());
        Parent = DesignTime.Shell;
    }

    public PropertyEditorViewModel(string id)
        : base(id)
    {
        _itemsSource = new ObservableList<IPropertyViewModel>();
        _itemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        Items = _itemsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Disposable.AddAction(() => _itemsSource.ClearWithItemsDispose());
    }

    public ObservableList<IPropertyViewModel> ItemsSource => _itemsSource;

    public NotifyCollectionChangedSynchronizedViewList<IPropertyViewModel> Items { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return _itemsSource;
    }
}
