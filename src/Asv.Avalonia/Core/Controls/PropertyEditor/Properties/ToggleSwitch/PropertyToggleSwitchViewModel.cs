using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyToggleSwitchViewModel : PropertyViewModel
{
    private readonly IUndoChangeSink<ValueUndoChange<bool>>? _undoValueSink;
    private bool _changesFromViewModel;
    private bool _lastValue;

    protected PropertyToggleSwitchViewModel(string typeId, bool enableUndo = true)
        : base(typeId)
    {
        IconColor = AsvColorKind.Info3;
        Value = new BindableReactiveProperty<bool>().AddTo(ref DisposableBag);
        Value
            .Skip(1)
            .SubscribeAwait((value, cancel) => SetValueFromUser(value, cancel), AwaitOperation.Drop)
            .AddTo(ref DisposableBag);

        if (enableUndo)
        {
            _undoValueSink = Undo.Register<ValueUndoChange<bool>>("Value", OnUndoValue, OnRedoValue)
                .AddTo(ref DisposableBag);
        }
    }

    public BindableReactiveProperty<bool> Value { get; }

    public string? CheckedText
    {
        get;
        set => SetField(ref field, value);
    } = "ON";

    public string? UncheckedText
    {
        get;
        set => SetField(ref field, value);
    } = "OFF";

    public MaterialIconKind? CheckedIcon
    {
        get;
        set => SetField(ref field, value);
    } = MaterialIconKind.ToggleSwitch;

    public MaterialIconKind? UncheckedIcon
    {
        get;
        set => SetField(ref field, value);
    } = MaterialIconKind.ToggleSwitchOff;

    public AsvColorKind CheckedStatus
    {
        get;
        set => SetField(ref field, value);
    } = AsvColorKind.Success;

    public AsvColorKind UncheckedStatus
    {
        get;
        set => SetField(ref field, value);
    } = AsvColorKind.Error;

    public ValueTask SetValueFromUser(bool value, CancellationToken cancel = default)
    {
        return SetValueFromUser(value, cancel, true);
    }

    protected void ApplyValueFromModel(bool newValue)
    {
        ClearModelErrors();
        _lastValue = newValue;
        ApplyValueToView(newValue);
        MarkUpdated();
    }

    protected abstract ValueTask ApplyFromUser(bool value, CancellationToken cancel);

    private async ValueTask SetValueFromUser(bool value, CancellationToken cancel, bool publishUndo)
    {
        if (_changesFromViewModel || IsBusy)
        {
            return;
        }

        var oldValue = _lastValue;
        ApplyValueToView(value);

        if (EqualityComparer<bool>.Default.Equals(oldValue, value))
        {
            return;
        }

        ClearModelErrors();
        IsBusy = true;
        try
        {
            await ApplyFromUser(value, cancel).ConfigureAwait(false);
            _lastValue = value;

            if (publishUndo)
            {
                _undoValueSink?.PublishUpdate(oldValue, value);
            }

            MarkUpdated();
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            ApplyValueToView(oldValue);
        }
        catch (Exception e)
        {
            ApplyValueToView(oldValue);
            ApplyErrorFromModel(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private ValueTask OnRedoValue(ValueUndoChange<bool> change, CancellationToken cancel)
    {
        return SetValueFromUser(change.NewValue, cancel, false);
    }

    private ValueTask OnUndoValue(ValueUndoChange<bool> change, CancellationToken cancel)
    {
        return SetValueFromUser(change.OldValue, cancel, false);
    }

    private void ApplyValueToView(bool value)
    {
        _changesFromViewModel = true;
        try
        {
            Value.Value = value;
        }
        finally
        {
            _changesFromViewModel = false;
        }
    }
}
