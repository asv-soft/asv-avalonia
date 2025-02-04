namespace Asv.Avalonia;

public class MeasureUnitViewModel : RoutableViewModel
{
    private IUnitItem _selectedItem;

    public MeasureUnitViewModel(IUnit item)
        : base(item.UnitId)
    {
        _selectedItem = item.Current.CurrentValue;
        Base = item;
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return new ValueTask<IRoutable>(this);
    }

    public IUnitItem SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public IUnit Base { get; }

    public bool Fitler(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return Base.Name.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}
