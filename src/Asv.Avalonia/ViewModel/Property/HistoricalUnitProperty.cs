using R3;

namespace Asv.Avalonia;

public class HistoricalUnitProperty : RoutableViewModel
{
    private readonly ReactiveProperty<double> _modelValue;
    private readonly IUnit _unit;
    private readonly string? _format;

    private bool _internalChange;
    private bool _userChange;

    public ReactiveProperty<double> ModelValue => _modelValue;
    public BindableReactiveProperty<string?> ViewValue { get; } = new();
    public BindableReactiveProperty<bool> IsSelected { get; } = new();
    public IUnit Unit => _unit;

    public HistoricalUnitProperty(
        string id,
        ReactiveProperty<double> modelValue,
        IUnit unit,
        string? format = null
    )
        : base(id)
    {
        _modelValue = modelValue;
        _unit = unit;
        _format = format;
        _internalChange = true;
        _sub2 = ViewValue
            .EnableValidation(ValidateValue)
            .SubscribeAwait(OnChangedByUser, AwaitOperation.Drop);
        _internalChange = false;
        _sub3 = _modelValue.Subscribe(OnChangeByModel);
        _sub4 = unit.Current.Subscribe(_ => OnChangeByModel(modelValue.CurrentValue));
    }

    private Exception? ValidateValue(string? userValue)
    {
        var result = _unit.Current.CurrentValue.ValidateValue(userValue);
        return result.IsSuccess ? null : result.ValidationException;
    }

    private async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _userChange = true;
        var value = _unit.Current.CurrentValue.ParseToSi(userValue);
        var newValue = new Persistable<double>(value);
        _modelValue.OnNext(value);
        await this.ExecuteCommand(ChangeDoublePropertyCommand.Id, newValue);
        _userChange = false;
    }

    private void OnChangeByModel(double modelValue)
    {
        if (_userChange)
        {
            return;
        }

        _internalChange = true;
        ViewValue.OnNext(_unit.Current.CurrentValue.PrintFromSi(modelValue, _format));
        _internalChange = false;
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        return ValueTask.CompletedTask;
    }

    public IPersistable Save()
    {
        return new Persistable<double>(ModelValue.Value);
    }

    public void Restore(IPersistable state)
    {
        if (state is Persistable<double> value)
        {
            ModelValue.OnNext(value.Value);
        }
    }

    #region Dispose

    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;
    private readonly IDisposable _sub4;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _sub2.Dispose();
        _sub3.Dispose();
        _sub4.Dispose();
        ViewValue.Dispose();
        IsSelected.Dispose();
    }

    #endregion
}
