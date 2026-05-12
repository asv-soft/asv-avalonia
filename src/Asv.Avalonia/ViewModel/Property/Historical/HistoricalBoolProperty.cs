using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalBoolProperty
    : BindablePropertyBase<bool, bool>,
        IHistoricalProperty<bool>
{
    private bool _internalChange;
    private readonly IUndoChangeSink<ValueUndoChange<bool>> _undoSink;

    public HistoricalBoolProperty(string typeId, ReactiveProperty<bool> modelValue)
        : base(typeId)
    {
        ModelValue = modelValue;
        ViewValue = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        ViewValue.EnableValidation(ValidateUserValue);

        _internalChange = true;
        ViewValue.SubscribeAwait(OnChangedByUser, AwaitOperation.Drop).DisposeItWith(Disposable);
        _internalChange = false;

        ModelValue.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
        _undoSink = Undo.CreateValueChange<bool>("default", ApplyBoolValue, ApplyBoolValue)
            .DisposeItWith(Disposable);
    }

    private void ApplyBoolValue(bool value)
    {
        ModelValue.Value = value;
    }

    public override ReactiveProperty<bool> ModelValue { get; }
    public override BindableReactiveProperty<bool> ViewValue { get; }

    protected override Exception? ValidateUserValue(bool userValue)
    {
        return null;
    }

    protected override async ValueTask OnChangedByUser(bool userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        var oldValue = ModelValue.Value;
        if (oldValue == userValue)
        {
            return;
        }

        try
        {
            _internalChange = true;
            ApplyBoolValue(userValue);
            _undoSink.Publish(oldValue, userValue);
        }
        finally
        {
            _internalChange = false;
        }
    }

    protected override void OnChangeByModel(bool modelValue)
    {
        _internalChange = true;
        ViewValue.OnNext(modelValue);
        _internalChange = false;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override ValueTask ApplyValueToModel(bool value, CancellationToken cancel)
    {
        ApplyBoolValue(value);
        return ValueTask.CompletedTask;
    }
}
