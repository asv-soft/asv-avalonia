using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;

namespace Asv.Avalonia;

public partial class CompactSlider : TemplatedControl
{
    private bool _coercingValue;
    private bool _dragging;

    static CompactSlider()
    {
        FocusableProperty.OverrideDefaultValue<CompactSlider>(true);
    }

    public CompactSlider()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Center;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        CoerceCurrentValue();
        UpdateMetrics();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        return new Size(width, Height > 0 ? Height : 24);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (
            change.Property == ValueProperty
            || change.Property == MinimumProperty
            || change.Property == MaximumProperty
            || change.Property == TickFrequencyProperty
            || change.Property == IsSnapToTickEnabledProperty
        )
        {
            CoerceCurrentValue();
            UpdateMetrics();
            return;
        }

        if (
            change.Property == BoundsProperty
            || change.Property == ThumbSizeProperty
            || change.Property == ThumbWidthProperty
            || change.Property == TrackHeightProperty
        )
        {
            UpdateMetrics();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var properties = e.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed)
        {
            return;
        }

        Focus();
        _dragging = true;
        e.Pointer.Capture(this);
        SetValueFromPoint(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_dragging)
        {
            return;
        }

        SetValueFromPoint(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        _dragging = false;
        base.OnPointerCaptureLost(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var handled = true;
        switch (e.Key)
        {
            case Key.Left:
            case Key.Down:
                SetCurrentValue(ValueProperty, CoerceValue(Value - SmallChange));
                break;

            case Key.Right:
            case Key.Up:
                SetCurrentValue(ValueProperty, CoerceValue(Value + SmallChange));
                break;

            case Key.PageDown:
                SetCurrentValue(ValueProperty, CoerceValue(Value - LargeChange));
                break;

            case Key.PageUp:
                SetCurrentValue(ValueProperty, CoerceValue(Value + LargeChange));
                break;

            case Key.Home:
                SetCurrentValue(ValueProperty, CoerceValue(Minimum));
                break;

            case Key.End:
                SetCurrentValue(ValueProperty, CoerceValue(Maximum));
                break;

            default:
                handled = false;
                break;
        }

        e.Handled = handled;
    }

    private void SetValueFromPoint(double x)
    {
        var trackWidth = GetTrackWidth();
        if (trackWidth <= 0)
        {
            return;
        }

        var ratio = Math.Clamp((x - GetThumbRadius()) / trackWidth, 0, 1);
        SetCurrentValue(ValueProperty, CoerceValue(Minimum + (ratio * GetRange())));
    }

    private void CoerceCurrentValue()
    {
        if (_coercingValue)
        {
            return;
        }

        var coerced = CoerceValue(Value);
        if (EqualityComparer<double>.Default.Equals(coerced, Value))
        {
            return;
        }

        _coercingValue = true;
        try
        {
            SetCurrentValue(ValueProperty, coerced);
        }
        finally
        {
            _coercingValue = false;
        }
    }

    private double CoerceValue(double value)
    {
        if (Maximum < Minimum)
        {
            return value;
        }

        var result = Math.Clamp(value, Minimum, Maximum);
        if (IsSnapToTickEnabled && TickFrequency > 0)
        {
            result = Minimum + (Math.Round((result - Minimum) / TickFrequency) * TickFrequency);
            result = Math.Clamp(result, Minimum, Maximum);
        }

        return result;
    }

    private void UpdateMetrics()
    {
        var thumbHeight = Math.Max(0, ThumbSize);
        var height = Bounds.Height > 0 ? Bounds.Height : 24;
        var trackWidth = GetTrackWidth();
        var ratio = GetValueRatio();
        var thumbLeft = ratio * trackWidth;

        ThumbTop = Math.Max(0, (height - thumbHeight) / 2);
        ThumbLeft = thumbLeft;
    }

    private double GetValueRatio()
    {
        var range = GetRange();
        if (range <= 0)
        {
            return 0;
        }

        return Math.Clamp((Value - Minimum) / range, 0, 1);
    }

    private double GetRange()
    {
        return Maximum - Minimum;
    }

    private double GetTrackWidth()
    {
        return Math.Max(0, Bounds.Width - GetThumbWidth());
    }

    private double GetThumbRadius()
    {
        return GetThumbWidth() / 2;
    }

    private double GetThumbWidth()
    {
        return Math.Max(Math.Max(0, ThumbSize), Math.Max(0, ThumbWidth));
    }
}
