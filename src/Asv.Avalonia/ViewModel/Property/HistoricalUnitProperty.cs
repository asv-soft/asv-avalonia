using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalUnitProperty : HistoricalPropertyBase<double, string?>
{
    private readonly ReactiveProperty<double> _modelValue;
    private readonly IUnit _unit;
    private readonly string? _format;

    private bool _internalChange;

    public override ReactiveProperty<double> ModelValue => _modelValue;
    public override BindableReactiveProperty<string?> ViewValue { get; } = new();
    public override BindableReactiveProperty<bool> IsSelected { get; } = new();
    public IUnit Unit => _unit;

    public HistoricalUnitProperty(
        string id,
        ReactiveProperty<double> modelValue,
        IUnit unit,
        ILoggerFactory loggerFactory,
        string? format = null
    )
        : base(id, loggerFactory)
    {
        _modelValue = modelValue;
        _unit = unit;
        _format = format;

        _internalChange = true;
        ViewValue.EnableValidation().ForceValidate();
        _sub2 = ViewValue.SubscribeAwait(
            async (value, cancel) =>
            {
                var error = ValidateValue(value);
                if (error is null)
                {
                    await OnChangedByUser(value, cancel);
                    return;
                }

                ViewValue.OnErrorResume(error);
            },
            AwaitOperation.Drop
        );
        _internalChange = false;

        _sub3 = _modelValue.Subscribe(OnChangeByModel);
        _sub4 = unit.CurrentUnitItem.Subscribe(_ => OnChangeByModel(modelValue.CurrentValue));
    }

    protected override Exception? ValidateValue(string? userValue)
    {
        var result = _unit.CurrentUnitItem.CurrentValue.ValidateValue(userValue);
        if (result.IsSuccess)
        {
            return null;
        }

        return result.ValidationException;
    }

    protected override async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        var value = _unit.CurrentUnitItem.CurrentValue.ParseToSi(userValue);
        var newValue = new DoubleArg(value);
        await this.ExecuteCommand(ChangeDoublePropertyCommand.Id, newValue);
    }

    protected override void OnChangeByModel(double modelValue)
    {
        _internalChange = true;
        ViewValue.OnNext(_unit.CurrentUnitItem.CurrentValue.PrintFromSi(modelValue, _format));
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        return ValueTask.CompletedTask;
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
