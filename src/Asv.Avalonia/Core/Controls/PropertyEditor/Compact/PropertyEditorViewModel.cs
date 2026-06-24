using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class PropertyEditorViewModel : ViewModel
{
    private readonly ObservableList<IPropertyViewModel> _itemsSource;
    private readonly ISynchronizedView<IPropertyViewModel, IPropertyViewModel> _itemsView;

    public PropertyEditorViewModel()
        : this(DesignTime.Id.TypeId)
    {
        DesignTime.ThrowIfNotDesignMode();
        _itemsSource.Add(new PropertyTextBoxDesign { ShortHeader = "A" });
        _itemsSource.Add(new PropertyComboBoxDesign { ShortHeader = "Ab" });
        _itemsSource.Add(new PropertyToggleButtonGroupDesign { ShortHeader = "Abc" });
        _itemsSource.Add(new PropertyUnitDesign { ShortHeader = "Abc" });
        _itemsSource.Add(new PropertyToggleSwitchDesign { ShortHeader = "Abcd" });
        _itemsSource.Add(new PropertyButtonViewModel() { ShortHeader = "Abcd" });

        SetParent(DesignTime.Shell);
    }

    public PropertyEditorViewModel(string id)
        : base(id)
    {
        _itemsSource = [];
        _itemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        _itemsView = _itemsSource.CreateView(x => x).DisposeItWith(Disposable);
        RefreshDisplayScopeFilter();
        DisplayScopes
            .ObserveCountChanged()
            .Subscribe(_ => RefreshDisplayScopeFilter())
            .AddTo(ref DisposableBag);
        Items = _itemsView.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        Disposable.AddAction(() => _itemsSource.ClearWithItemsDispose());
    }

    public ObservableList<IPropertyViewModel> ItemsSource => _itemsSource;

    public NotifyCollectionChangedSynchronizedViewList<IPropertyViewModel> Items { get; }

    public bool ShowHeader
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return _itemsSource;
    }

    public ObservableHashSet<string> DisplayScopes { get; } = [];

    private void RefreshDisplayScopeFilter()
    {
        _itemsView.AttachFilter(
            new SynchronizedViewFilter<IPropertyViewModel, IPropertyViewModel>(
                (_, property) => ShouldShowProperty(property)
            )
        );
    }

    private bool ShouldShowProperty(IPropertyViewModel property)
    {
        if (property.DisplayScopes.Count == 0)
        {
            return true;
        }

        if (DisplayScopes.Count == 0)
        {
            return false;
        }

        foreach (var scope in property.DisplayScopes)
        {
            if (DisplayScopes.Contains(scope))
            {
                return true;
            }
        }

        return false;
    }
}
