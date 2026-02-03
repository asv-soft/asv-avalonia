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
        ILoggerFactory loggerFactory,
        string? format = null
    )
        : base(id, loggerFactory)
    {
        Unit = unit;

        _format = format;

        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateUserValue);

        _internalChange = true;
        ViewValue
            .Skip(1) // this is for first change
            .SubscribeAwait(OnChangedByUser, AwaitOperation.Drop)
            .DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);

        unit.CurrentUnitItem.Subscribe(_ => OnChangeByModel(modelValue.CurrentValue))
            .DisposeItWith(Disposable);
    }

    public sealed override ReactiveProperty<double> ModelValue { get; }
    public sealed override BindableReactiveProperty<string?> ViewValue { get; }
    public IUnit Unit { get; }

    protected override Exception? ValidateUserValue(string? userValue)
    {
        var result = Unit.CurrentUnitItem.CurrentValue.ValidateValue(userValue);

        if (result.IsSuccess)
        {
            var resultSi = ValidateSiValue(Unit.CurrentUnitItem.CurrentValue.ParseToSi(userValue));
            return resultSi;
        }

        Logger.LogError(result.ValidationException, "Validation error");
        return result.ValidationException?.GetExceptionWithLocalizationOrSelf();
    }

    protected virtual Exception? ValidateSiValue(double siValue)
    {
        return null;
    }

    protected override async ValueTask OnChangedByUser(string? userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        if (ViewValue.HasErrors)
        {
            // we don't apply value if view has errors
            return;
        }

        _externalChange = true;
        var value = Unit.CurrentUnitItem.CurrentValue.ParseToSi(userValue);
        await ApplyValueToModel(value, cancel);
        _externalChange = false;
    }

    protected override void OnChangeByModel(double modelValue)
    {
        if (_externalChange)
        {
            return;
        }

        _internalChange = true;
        var stringValue = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(modelValue, _format);

        ViewValue.OnNext(stringValue);
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
