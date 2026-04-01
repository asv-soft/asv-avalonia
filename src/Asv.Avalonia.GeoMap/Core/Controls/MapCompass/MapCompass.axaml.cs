using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Asv.Avalonia.GeoMap;

public class MapCompass : TemplatedControl
{
    private Cursor? _handCursor;
    private bool _isDragging;
    private bool _isNormalizingRotation;
    private double _dragStartAngle;
    private double _dragStartRotation;
    private IPointer? _activePointer;
    private InputElement? _gestureSurface;
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

        MouseRotationSensitivityProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is MapCompass compass)
            {
                compass.NormalizeNonNegative(MouseRotationSensitivityProperty);
            }
        });

        TouchpadRotationSensitivityProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is MapCompass compass)
            {
                compass.NormalizeNonNegative(TouchpadRotationSensitivityProperty);
            }
        });
    }

    public static readonly StyledProperty<double> RotationProperty = AvaloniaProperty.Register<
        MapCompass,
        double
    >(nameof(Rotation), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> DeadZoneProperty = AvaloniaProperty.Register<
        MapCompass,
        double
    >(nameof(DeadZone), 0);

    public static readonly StyledProperty<bool> EnableTouchpadGesturesProperty =
        AvaloniaProperty.Register<MapCompass, bool>(nameof(EnableTouchpadGestures), true);

    public static readonly StyledProperty<double> MouseRotationSensitivityProperty =
        AvaloniaProperty.Register<MapCompass, double>(nameof(MouseRotationSensitivity), 1.0);

    public static readonly StyledProperty<double> TouchpadRotationSensitivityProperty =
        AvaloniaProperty.Register<MapCompass, double>(nameof(TouchpadRotationSensitivity), 1.0);

    public MapCompass()
    {
        AddHandler(Gestures.PointerTouchPadGestureRotateEvent, OnTouchPadGestureRotate);
    }

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public double DeadZone
    {
        get => GetValue(DeadZoneProperty);
        set => SetValue(DeadZoneProperty, value);
    }

    public bool EnableTouchpadGestures
    {
        get => GetValue(EnableTouchpadGesturesProperty);
        set => SetValue(EnableTouchpadGesturesProperty, value);
    }

    public double MouseRotationSensitivity
    {
        get => GetValue(MouseRotationSensitivityProperty);
        set => SetValue(MouseRotationSensitivityProperty, Math.Abs(value));
    }

    public double TouchpadRotationSensitivity
    {
        get => GetValue(TouchpadRotationSensitivityProperty);
        set => SetValue(TouchpadRotationSensitivityProperty, Math.Abs(value));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var needleHost = e.NameScope.Find<Control>("PART_NeedleHost");
        _needleRotation = needleHost?.RenderTransform as RotateTransform;
        UpdateNeedleRotation();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachToGestureSurface();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var properties = e.GetCurrentPoint(this).Properties;
        if (properties is { IsLeftButtonPressed: false, IsRightButtonPressed: false })
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            Rotation = 0;
            e.Handled = true;
            return;
        }

        var position = GetPositionOnGestureSurface(e);
        if (!TryGetPointerAngle(position, out var angle))
        {
            return;
        }

        BeginDrag(e.Pointer, this, angle);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isDragging || _activePointer != e.Pointer)
        {
            return;
        }

        var position = GetPositionOnGestureSurface(e);
        if (!TryGetPointerAngle(position, out var angle))
        {
            return;
        }

        Rotation =
            _dragStartRotation
            + (NormalizeDelta(angle - _dragStartAngle) * MouseRotationSensitivity);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_activePointer == e.Pointer)
        {
            EndDrag(e.Pointer);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_activePointer == e.Pointer)
        {
            EndDrag(null);
        }
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
        _needleRotation?.Angle = Rotation;
    }

    private void EndDrag(IPointer? pointer)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        var pointerToRelease = pointer ?? _activePointer;
        _activePointer = null;
        Cursor = Cursor.Default;
        pointerToRelease?.Capture(null);
    }

    private void OnTouchPadGestureRotate(object? sender, PointerDeltaEventArgs e)
    {
        if (!EnableTouchpadGestures)
        {
            return;
        }

        var dominantDelta = Math.Abs(e.Delta.X) >= Math.Abs(e.Delta.Y) ? e.Delta.X : e.Delta.Y;
        Rotation += dominantDelta * TouchpadRotationSensitivity;
        e.Handled = true;
    }

    private bool TryGetPointerAngle(Point positionOnSurface, out double angle)
    {
        if (GetGestureSurfaceVisual() is not { } surfaceVisual)
        {
            angle = 0;
            return false;
        }

        var center = new Point(surfaceVisual.Bounds.Width * 0.5, surfaceVisual.Bounds.Height * 0.5);
        var vector = positionOnSurface - center;
        var deadZone = Math.Max(0, DeadZone);
        if (vector.LengthSquared() < deadZone * deadZone)
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

    private void OnSurfacePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_isDragging)
        {
            return;
        }

        var properties = e.GetCurrentPoint(_gestureSurface ?? this).Properties;
        if (!properties.IsRightButtonPressed)
        {
            return;
        }

        var position = GetPositionOnGestureSurface(e);
        if (!TryGetPointerAngle(position, out var angle))
        {
            return;
        }

        BeginDrag(e.Pointer, _gestureSurface ?? this, angle);
        e.Handled = true;
    }

    private void OnSurfacePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _activePointer != e.Pointer)
        {
            return;
        }

        var position = GetPositionOnGestureSurface(e);
        if (!TryGetPointerAngle(position, out var angle))
        {
            return;
        }

        Rotation =
            _dragStartRotation
            + (NormalizeDelta(angle - _dragStartAngle) * MouseRotationSensitivity);
        e.Handled = true;
    }

    private void OnSurfacePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_activePointer != e.Pointer)
        {
            return;
        }

        EndDrag(e.Pointer);
        e.Handled = true;
    }

    private void OnSurfacePointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_activePointer != e.Pointer)
        {
            return;
        }

        EndDrag(null);
    }

    private void AttachToGestureSurface()
    {
        DetachFromGestureSurface();

        _gestureSurface = Parent as InputElement;
        if (_gestureSurface == null)
        {
            return;
        }

        _gestureSurface.AddHandler(PointerPressedEvent, OnSurfacePointerPressed);
        _gestureSurface.AddHandler(PointerMovedEvent, OnSurfacePointerMoved);
        _gestureSurface.AddHandler(PointerReleasedEvent, OnSurfacePointerReleased);
        _gestureSurface.AddHandler(PointerCaptureLostEvent, OnSurfacePointerCaptureLost);
        _gestureSurface.AddHandler(
            Gestures.PointerTouchPadGestureRotateEvent,
            OnTouchPadGestureRotate
        );
    }

    private void DetachFromGestureSurface()
    {
        if (_gestureSurface is null)
        {
            return;
        }

        _gestureSurface.RemoveHandler(PointerPressedEvent, OnSurfacePointerPressed);
        _gestureSurface.RemoveHandler(PointerMovedEvent, OnSurfacePointerMoved);
        _gestureSurface.RemoveHandler(PointerReleasedEvent, OnSurfacePointerReleased);
        _gestureSurface.RemoveHandler(PointerCaptureLostEvent, OnSurfacePointerCaptureLost);
        _gestureSurface.RemoveHandler(
            Gestures.PointerTouchPadGestureRotateEvent,
            OnTouchPadGestureRotate
        );
        _gestureSurface = null;
    }

    private void BeginDrag(IPointer pointer, IInputElement captureHost, double startAngle)
    {
        _isDragging = true;
        _activePointer = pointer;
        _dragStartAngle = startAngle;
        _dragStartRotation = Rotation;
        Cursor = _handCursor ??= new Cursor(StandardCursorType.Hand);
        pointer.Capture(captureHost);
    }

    private Point GetPositionOnGestureSurface(PointerEventArgs e)
    {
        var surface = GetGestureSurfaceVisual();
        return surface == null ? e.GetPosition(this) : e.GetPosition(surface);
    }

    private Visual? GetGestureSurfaceVisual()
    {
        return _gestureSurface as Visual ?? Parent as Visual ?? this;
    }

    private static void DisposeAndResetCursor(ref Cursor? cursor)
    {
        cursor?.Dispose();
        cursor = null;
    }

    private void NormalizeNonNegative(StyledProperty<double> property)
    {
        var value = GetValue(property);
        if (value < 0.0)
        {
            SetCurrentValue(property, Math.Abs(value));
        }
    }

    #region Dispose

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachFromGestureSurface();
        EndDrag(_activePointer);
        _needleRotation = null;
        DisposeAndResetCursor(ref _handCursor);
        base.OnDetachedFromVisualTree(e);
    }

    #endregion
}
