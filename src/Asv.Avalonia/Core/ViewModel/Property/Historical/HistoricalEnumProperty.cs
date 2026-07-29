using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalEnumProperty<TEnum>
    : BindablePropertyBase<TEnum, TEnum>,
        IHistoricalProperty<TEnum>
    where TEnum : struct, Enum
{
    private bool _internalChange;
    private readonly IUndoChangeSink<ValueUndoChange<TEnum>> _undoSink;

    public HistoricalEnumProperty(string typeId, ReactiveProperty<TEnum> modelValue)
        : base(typeId)
    {
        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<TEnum>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateUserValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
        _undoSink = Undo.RegisterValue<TEnum>("default", ApplyEnumValue, ApplyEnumValue)
            .DisposeItWith(Disposable);
    }

    private void ApplyEnumValue(TEnum value)
    {
        ModelValue.Value = value;
    }

    public override ReactiveProperty<TEnum> ModelValue { get; }
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
            _undoSink.PublishUpdate(oldValue, userValue);
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

    protected override void OnChangeByModel(TEnum modelValue)
    {
        _internalChange = true;
        ViewValue.OnNext(modelValue);
        _internalChange = false;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
