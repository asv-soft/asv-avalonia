using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class ValidationEnumProperty<TEnum>
    : ValidationPropertyBase<Enum, TEnum>,
        IValidationProperty<Enum>
    where TEnum : struct, Enum
{
    private bool _externalChange;
    private bool _internalChange;

    public ValidationEnumProperty(
        NavigationId id,
        ReactiveProperty<Enum> modelValue,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(id, loggerFactory, parent)
    {
        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<TEnum>().DisposeItWith(Disposable);
        IsSelected = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
    }

    public sealed override ReactiveProperty<Enum> ModelValue { get; }
    public sealed override BindableReactiveProperty<TEnum> ViewValue { get; }
    public sealed override BindableReactiveProperty<bool> IsSelected { get; }

    public TEnum[] EnumItems => Enum.GetValues<TEnum>();

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override Exception? ValidateValue(TEnum userValue)
    {
        return null;
    }

    protected override async ValueTask OnChangedByUser(TEnum userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _externalChange = true;
        await ChangeModelValue(userValue, cancel);
        _externalChange = false;
    }

    protected override void OnChangeByModel(Enum modelValue)
    {
        if (_externalChange)
        {
            return;
        }

        _internalChange = true;
        if (modelValue is not TEnum newEnum)
        {
            throw new Exception($"{modelValue} is not a valid enum type for {nameof(TEnum)}");
        }

        ViewValue.OnNext(newEnum);
        _internalChange = false;
    }
}
