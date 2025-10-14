using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class BindableUnitProperty : BindablePropertyBase<double, string?>
{
    private readonly string? _format;
    private bool _externalChange;
    private bool _internalChange;

    public BindableUnitProperty(
        string id,
        ReactiveProperty<double> modelValue,
        IUnit unit,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IRoutable parent,
        string? format = null
    )
        : base(id, layoutService, loggerFactory, parent)
    {
        Unit = unit;
        _format = format;

        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        IsSelected = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);

        unit.CurrentUnitItem.Subscribe(_ => OnChangeByModel(modelValue.CurrentValue))
            .DisposeItWith(Disposable);
    }

    public sealed override ReactiveProperty<double> ModelValue { get; }
    public sealed override BindableReactiveProperty<string?> ViewValue { get; }
    public sealed override BindableReactiveProperty<bool> IsSelected { get; }
    public IUnit Unit { get; }

    protected override Exception? ValidateValue(string? userValue)
    {
        var result = Unit.CurrentUnitItem.CurrentValue.ValidateValue(userValue);
        return result.IsSuccess ? null : result.ValidationException;
    }

    protected override async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _externalChange = true;
        var value = Unit.CurrentUnitItem.CurrentValue.ParseToSi(userValue);
        await ChangeModelValue(value, cancel);
        _externalChange = false;
    }

    protected override void OnChangeByModel(double modelValue)
    {
        if (_externalChange)
        {
            return;
        }

        _internalChange = true;
        ViewValue.OnNext(Unit.CurrentUnitItem.CurrentValue.PrintFromSi(modelValue, _format));
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
