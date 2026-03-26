using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Asv.Avalonia.GeoMap;

public class MapCompass : TemplatedControl
{
    private const double CenterDeadZone = 16.0;

    private bool _isDragging;
    private bool _isNormalizingRotation;
    private double _dragStartAngle;
    private double _dragStartRotation;
    private RotateTransform? _needleRotation;

    static MapCompass()
    {
        RotationProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is MapCompass compass)
            {
                compass.NormalizeRotation();
                compass.UpdateNeedleRotation();
            }
        });
    }

    public static readonly StyledProperty<double> RotationProperty =
        AvaloniaProperty.Register<MapCompass, double>(
            nameof(Rotation),
            defaultBindingMode: BindingMode.TwoWay
        );

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var needleHost = e.NameScope.Find<Control>("PART_NeedleHost");
        _needleRotation = needleHost?.RenderTransform as RotateTransform;
        UpdateNeedleRotation();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            Rotation = 0;
            e.Handled = true;
            return;
        }

        if (!TryGetPointerAngle(e.GetPosition(this), out var angle))
        {
            return;
        }

        _isDragging = true;
        _dragStartAngle = angle;
        _dragStartRotation = Rotation;
        Cursor = new Cursor(StandardCursorType.Hand);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isDragging)
        {
            return;
        }

        if (!TryGetPointerAngle(e.GetPosition(this), out var angle))
        {
            return;
        }

        Rotation = _dragStartRotation + NormalizeDelta(angle - _dragStartAngle);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        EndDrag(e.Pointer);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndDrag(null);
    }

    private void NormalizeRotation()
    {
        if (_isNormalizingRotation)
        {
            return;
        }

        var normalized = NormalizeAngle(GetValue(RotationProperty));
        if (Math.Abs(normalized - GetValue(RotationProperty)) < 0.001)
        {
            return;
        }

        _isNormalizingRotation = true;
        try
        {
            SetCurrentValue(RotationProperty, normalized);
        }
        finally
        {
            _isNormalizingRotation = false;
        }
    }

    private void UpdateNeedleRotation()
    {
        if (_needleRotation != null)
        {
            _needleRotation.Angle = Rotation;
        }
    }

    private void EndDrag(IPointer? pointer)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        Cursor = Cursor.Default;
        pointer?.Capture(null);
    }

    private bool TryGetPointerAngle(Point position, out double angle)
    {
        var center = new Point(Bounds.Width * 0.5, Bounds.Height * 0.5);
        var vector = position - center;
        if (vector.LengthSquared() < CenterDeadZone * CenterDeadZone)
        {
            angle = 0;
            return false;
        }

        angle = NormalizeAngle((Math.Atan2(vector.Y, vector.X) * 180.0 / Math.PI) + 90.0);
        return true;
    }

    private static double NormalizeAngle(double angle)
    {
        var normalized = angle % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }

    private static double NormalizeDelta(double delta)
    {
        if (delta > 180.0)
        {
            return delta - 360.0;
        }

        return delta < -180.0 ? delta + 360.0 : delta;
    }
}
