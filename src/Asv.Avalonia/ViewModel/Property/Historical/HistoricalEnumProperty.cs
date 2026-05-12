using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalEnumProperty<TEnum>
    : BindablePropertyBase<Enum, TEnum>,
        IHistoricalProperty<Enum>
    where TEnum : struct, Enum
{
    private bool _internalChange;
    private readonly IUndoChangeSink<ValueUndoChange<Enum>> _undoSink;

    public HistoricalEnumProperty(string typeId, ReactiveProperty<Enum> modelValue)
        : base(typeId)
    {
        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<TEnum>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateUserValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
        _undoSink = Undo.CreateValueChange<Enum>("default", ApplyEnumValue, ApplyEnumValue)
            .DisposeItWith(Disposable);
    }

    private void ApplyEnumValue(Enum value)
    {
        ModelValue.Value = value;
    }

    public override ReactiveProperty<Enum> ModelValue { get; }
    public override BindableReactiveProperty<TEnum> ViewValue { get; }

    public TEnum[] EnumItems => Enum.GetValues<TEnum>();

    protected override Exception? ValidateUserValue(TEnum userValue)
    {
        return null;
    }

    protected override ValueTask OnChangedByUser(TEnum userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return ValueTask.CompletedTask;
        }

        var oldValue = ModelValue.Value;
        if (oldValue.Equals(userValue))
        {
            return ValueTask.CompletedTask;
        }

        try
        {
            _internalChange = true;
            ApplyEnumValue(userValue);
            _undoSink.Publish(oldValue, userValue);
            return ValueTask.CompletedTask;
        }
        catch (Exception exception)
        {
            return ValueTask.FromException(exception);
        }
        finally
        {
            _internalChange = false;
        }
    }

    protected override void OnChangeByModel(Enum modelValue)
    {
        _internalChange = true;
        ViewValue.OnNext(GetViewValue(modelValue));
        _internalChange = false;
    }

    private static TEnum GetViewValue(Enum modelValue)
    {
        if (modelValue is not TEnum newEnum)
        {
            throw new Exception($"{modelValue} is not a valid enum type for {nameof(TEnum)}");
        }

        return newEnum;
    }

    protected override ValueTask ApplyValueToModel(Enum value, CancellationToken cancel)
    {
        ApplyEnumValue(value);
        return ValueTask.CompletedTask;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
