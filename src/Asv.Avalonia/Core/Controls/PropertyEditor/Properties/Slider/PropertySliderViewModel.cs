using System.Globalization;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public abstract class PropertySliderViewModel : PropertyViewModel
{
    private readonly IUndoChangeSink<ValueUndoChange<double>>? _undoValueSink;
    private bool _changesFromViewModel;
    private double _lastValue;

    protected PropertySliderViewModel(
        string typeId,
        double minimum = 0,
        double maximum = 100,
        bool enableUndo = true
    )
        : base(typeId)
    {
        IconColor = AsvColorKind.Info3;
        Minimum = minimum;
        Maximum = maximum;
        Value = new BindableReactiveProperty<double>().AddTo(ref DisposableBag);
        Value
            .Skip(1)
            .SubscribeAwait((value, cancel) => SetValueFromUser(value, cancel), AwaitOperation.Drop)
            .AddTo(ref DisposableBag);

        if (enableUndo)
        {
            _undoValueSink = Undo.Register<ValueUndoChange<double>>(
                    "Value",
                    OnUndoValue,
                    OnRedoValue
                )
                .AddTo(ref DisposableBag);
        }

        UpdateDisplayValue(Value.Value);
    }

    private ValueTask OnRedoValue(ValueUndoChange<double> change, CancellationToken cancel)
    {
        return SetValueFromUser(change.NewValue, cancel, false);
    }

    private ValueTask OnUndoValue(ValueUndoChange<double> change, CancellationToken cancel)
    {
        return SetValueFromUser(change.OldValue, cancel, false);
    }

    public BindableReactiveProperty<double> Value { get; }

    public double Minimum
    {
        get;
        set => SetField(ref field, value);
    }

    public double Maximum
    {
        get;
        set => SetField(ref field, value);
    } = 100;

    public double TickFrequency
    {
        get;
        set => SetField(ref field, value);
    } = 1;

    public double SmallChange
    {
        get;
        set => SetField(ref field, value);
    } = 1;

    public double LargeChange
    {
        get;
        set => SetField(ref field, value);
    } = 10;

    public bool IsSnapToTickEnabled
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Units
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ValueFormat
    {
        get;
        set
        {
            if (SetField(ref field, value))
            {
                UpdateDisplayValue(Value.Value);
            }
        }
    } = "0.##";

    public string DisplayValue
    {
        get;
        private set => SetField(ref field, value);
    } = string.Empty;

    public ValueTask SetValueFromUser(double value, CancellationToken cancel = default)
    {
        return SetValueFromUser(value, cancel, true);
    }

    protected void ApplyValueFromModel(double newValue)
    {
        ClearModelErrors();
        var coercedValue = CoerceValue(newValue);
        _lastValue = coercedValue;
        ApplyValueToView(coercedValue);
        MarkUpdated();
    }

    protected abstract ValueTask ApplyFromUser(double value, CancellationToken cancel);

    private async ValueTask SetValueFromUser(
        double value,
        CancellationToken cancel,
        bool publishUndo
    )
    {
        if (_changesFromViewModel || IsBusy)
        {
            return;
        }

        var oldValue = _lastValue;
        var newValue = CoerceValue(value);
        ApplyValueToView(newValue);

        if (EqualityComparer<double>.Default.Equals(oldValue, newValue))
        {
            return;
        }

        ClearModelErrors();
        IsBusy = true;
        try
        {
            await ApplyFromUser(newValue, cancel).ConfigureAwait(false);
            _lastValue = newValue;

            if (publishUndo)
            {
                _undoValueSink?.PublishUpdate(oldValue, newValue);
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

    private double CoerceValue(double value)
    {
        if (Maximum < Minimum)
        {
            return value;
        }

        return Math.Clamp(value, Minimum, Maximum);
    }

    private void ApplyValueToView(double value)
    {
        _changesFromViewModel = true;
        try
        {
            Value.Value = value;
            UpdateDisplayValue(value);
        }
        finally
        {
            _changesFromViewModel = false;
        }
    }

    private void UpdateDisplayValue(double value)
    {
        DisplayValue = string.IsNullOrWhiteSpace(ValueFormat)
            ? value.ToString(CultureInfo.CurrentCulture)
            : value.ToString(ValueFormat, CultureInfo.CurrentCulture);
    }
}
