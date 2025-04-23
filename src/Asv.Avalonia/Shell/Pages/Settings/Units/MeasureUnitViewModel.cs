using R3;

namespace Asv.Avalonia;

public class MeasureUnitViewModel : RoutableViewModel
{
    private bool _internalChange;

    public MeasureUnitViewModel(IUnit item)
        : base(item.UnitId)
    {
        SelectedItem = new BindableReactiveProperty<IUnitItem>(item.CurrentUnitItem.CurrentValue);
        Base = item;
        _internalChange = true;
        _sub1 = SelectedItem.SubscribeAwait(OnChangedByUser);
        _sub2 = item.CurrentUnitItem.Subscribe(OnChangeByModel);
        _internalChange = false;
    }

    private async ValueTask OnChangedByUser(IUnitItem userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _internalChange = true;
        var newValue = new ActionCommandArg(
            Base.UnitId,
            userValue.UnitItemId,
            CommandParameterActionType.Change
        );
        await this.ExecuteCommand(ChangeCurrentUnitItemCommand.Id, newValue);
        _internalChange = false;
    }

    private void OnChangeByModel(IUnitItem modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = modelValue;
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public BindableReactiveProperty<IUnitItem> SelectedItem { get; }

    public IUnit Base { get; }

    public bool Filter(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return Base.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || Base.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
            || Base.CurrentUnitItem.CurrentValue.Name.Contains(
                search,
                StringComparison.OrdinalIgnoreCase
            )
            || Base.CurrentUnitItem.CurrentValue.Description.Contains(
                search,
                StringComparison.OrdinalIgnoreCase
            )
            || Base.CurrentUnitItem.CurrentValue.Symbol.Contains(
                search,
                StringComparison.OrdinalIgnoreCase
            )
            || Base.AvailableUnits.Values.First(u => u.IsInternationalSystemUnit)
                .Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || Base.AvailableUnits.Values.First(u => u.IsInternationalSystemUnit)
                .Description.Contains(search, StringComparison.OrdinalIgnoreCase)
            || Base.AvailableUnits.Values.First(u => u.IsInternationalSystemUnit)
                .Symbol.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            SelectedItem.Dispose();
            Base.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
