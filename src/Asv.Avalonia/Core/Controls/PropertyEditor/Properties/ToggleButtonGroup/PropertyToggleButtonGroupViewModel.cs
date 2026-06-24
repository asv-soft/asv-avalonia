using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyToggleButtonGroupViewModel : PropertyComboBoxViewModel
{
    private readonly ObservableList<PropertyToggleButtonGroupItemViewModel> _buttonItemsSource = [];

    protected PropertyToggleButtonGroupViewModel(string typeId, bool enableUndo = true)
        : base(typeId, enableUndo)
    {
        ButtonItemsView = _buttonItemsSource
            .ToNotifyCollectionChangedSlim()
            .AddTo(ref DisposableBag);

        ItemsSource.ObserveAdd().Subscribe(x => AddButtonItem(x.Value)).AddTo(ref DisposableBag);
        ItemsSource
            .ObserveRemove()
            .Subscribe(x => RemoveButtonItem(x.Value))
            .AddTo(ref DisposableBag);
        ItemsSource
            .ObserveClear()
            .Subscribe(_ => _buttonItemsSource.Clear())
            .AddTo(ref DisposableBag);
        SelectedItem.Subscribe(_ => RefreshSelectedItems()).AddTo(ref DisposableBag);
    }

    public NotifyCollectionChangedSynchronizedViewList<PropertyToggleButtonGroupItemViewModel> ButtonItemsView { get; }

    private void AddButtonItem(IHeadlinedViewModel item)
    {
        _buttonItemsSource.Add(
            new PropertyToggleButtonGroupItemViewModel(item)
            {
                IsSelected = IsSameItem(item, SelectedItem.Value),
            }
        );
    }

    private void RemoveButtonItem(IHeadlinedViewModel item)
    {
        var buttonItem =
            _buttonItemsSource.FirstOrDefault(x => ReferenceEquals(x.Item, item))
            ?? _buttonItemsSource.FirstOrDefault(x => IsSameItem(x.Item, item));
        if (buttonItem is null)
        {
            return;
        }

        _buttonItemsSource.Remove(buttonItem);
    }

    private void RefreshSelectedItems()
    {
        foreach (var buttonItem in _buttonItemsSource)
        {
            buttonItem.IsSelected = IsSameItem(buttonItem.Item, SelectedItem.Value);
        }
    }

    private static bool IsSameItem(IHeadlinedViewModel? left, IHeadlinedViewModel? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left is IViewModel leftViewModel && right is IViewModel rightViewModel)
        {
            return leftViewModel.Id == rightViewModel.Id;
        }

        return EqualityComparer<IHeadlinedViewModel>.Default.Equals(left, right);
    }
}
