using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Input;

namespace Asv.Avalonia.GeoMap;

public enum DragState
{
    None,
    DragSelection,
    DragMap,
    SelectRectangle,
}

public partial class MapItemsControl : SelectingItemsControl
{
    private Cursor? _handCursor;
    private Cursor? _sizeAllCursor;
    private Cursor? _crossCursor;

    private Point _lastMousePosition;
    private Point _startMousePosition;
    private IPointer? _activePointer;
    private Control? _selectedContainer;
    private double _touchpadZoomAccumulator;

    public MapItemsControl()
    {
        SelectionMode = SelectionMode.Multiple;
        SelectionChanged += OnSelectionChanged;

        AddHandler(Gestures.PointerTouchPadGestureSwipeEvent, OnTouchPadGestureSwipe);
        AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, OnTouchPadGestureMagnify);
    }

    #region Property events

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (
            change.Property == MinZoomProperty
            || change.Property == MaxZoomProperty
            || change.Property == ProviderProperty // Some providers may have their own restrictions to the Zoom property
        )
        {
            SetCurrentValue(ZoomProperty, Zoom);
        }

        if (change.Property == TouchpadZoomSensitivityProperty && TouchpadZoomSensitivity < 0.0)
        {
            SetCurrentValue(TouchpadZoomSensitivityProperty, Math.Abs(TouchpadZoomSensitivity));
        }
    }

    #endregion

    #region Selection

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        foreach (var item in args.RemovedItems)
        {
            if (ContainerFromItem(item) is ISelectable sel)
            {
                sel.IsSelected = false;
            }
        }

        foreach (var item in args.AddedItems)
        {
            if (ContainerFromItem(item) is ISelectable sel)
            {
                sel.IsSelected = true;
            }
        }
    }

    public new IList? SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }

    /// <inheritdoc />
    public new ISelectionModel Selection
    {
        get => base.Selection;
        set => base.Selection = value;
    }

    /// <summary>Gets or sets the selection mode.</summary>
    /// <remarks>
    /// Note that the selection mode only applies to selections made via user interaction.
    /// Multiple selections can be made programmatically regardless of the value of this property.
    /// </remarks>
    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }

    #endregion

    protected override Control CreateContainerForItemOverride(
        object? item,
        int index,
        object? recycleKey
    )
    {
        return new MapItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<MapItem>(item, out recycleKey);
    }

    #region Pointer Events

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var position = e.GetPosition(this);
        _startMousePosition = _lastMousePosition = position;
        UpdateCursorLocation(position);

        var properties = e.GetCurrentPoint(this).Properties;

        if (!properties.IsLeftButtonPressed)
        {
            return;
        }

        switch (e.KeyModifiers)
        {
            case KeyModifiers.Shift:
                BeginSelectionRectangleDrag(e.Pointer, position);
                e.Handled = true;
                return;
            case KeyModifiers.Control
                when GetContainerFromEventSource(e.Source) is { } selectionContainer:
                BeginSelectionDrag(e.Pointer, position, selectionContainer);
                e.Handled = true;
                return;
        }

        BeginMapDrag(e.Pointer, position);

        if (GetContainerFromEventSource(e.Source) is { } container)
        {
            var index = IndexFromContainer(container);
            Selection.BeginBatchUpdate();
            try
            {
                Selection.Clear();
                Selection.Select(index);
            }
            finally
            {
                Selection.EndBatchUpdate();
            }
        }

        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var position = e.GetPosition(this);
        UpdateCursorLocation(position);

        if (DragState == DragState.None || _activePointer != e.Pointer)
        {
            return;
        }

        switch (DragState)
        {
            case DragState.DragMap:
                DragMapCenter(position - _lastMousePosition);
                break;
            case DragState.SelectRectangle:
                UpdateSelectRectangle(position);
                InvalidateVisual();
                break;
            case DragState.DragSelection:
                DragSelectedItems(position - _lastMousePosition);
                _selectedContainer = null;
                break;
            case DragState.None:
            default:
                break;
        }

        _lastMousePosition = position;
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_activePointer != e.Pointer)
        {
            return;
        }

        var releasePosition = e.GetPosition(this);

        var wasLeftClick =
            e.InitialPressMouseButton == MouseButton.Left
            && DragState == DragState.DragMap
            && (releasePosition - _startMousePosition).LengthSquared() <= ClickThresholdSquared;

        ClearDragState(e.Pointer);
        e.Handled = true;

        if (!wasLeftClick)
        {
            return;
        }

        var args = new MapClickEventArgs(
            MapClickEvent,
            this,
            CursorPosition,
            e.KeyModifiers,
            MouseButton.Left
        );
        RaiseEvent(args);
    }

    private const double ClickThresholdSquared = 16.0;

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        ClearDragState(null);
    }

    private void BeginMapDrag(IPointer pointer, Point position)
    {
        _activePointer = pointer;
        _lastMousePosition = position;
        DragState = DragState.DragMap;
        UpdateCursorForDragState();
        pointer.Capture(this);
    }

    private void BeginSelectionDrag(IPointer pointer, Point position, Control container)
    {
        _activePointer = pointer;
        _lastMousePosition = position;
        _selectedContainer = container;
        DragState = DragState.DragSelection;
        UpdateCursorForDragState();
        pointer.Capture(this);

        var index = IndexFromContainer(container);
        if (!Selection.IsSelected(index))
        {
            Selection.Clear();
            Selection.Select(index);
        }
    }

    private void BeginSelectionRectangleDrag(IPointer pointer, Point position)
    {
        _activePointer = pointer;
        _lastMousePosition = position;
        _startMousePosition = position;
        DragState = DragState.SelectRectangle;
        UpdateCursorForDragState();
        pointer.Capture(this);
        UpdateSelectRectangle(position);
    }

    private void ClearDragState(IPointer? pointer)
    {
        if (_selectedContainer != null)
        {
            Selection.BeginBatchUpdate();
            try
            {
                Selection.Clear();
                Selection.Select(IndexFromContainer(_selectedContainer));
            }
            finally
            {
                Selection.EndBatchUpdate();
            }

            _selectedContainer = null;
        }

        DragState = DragState.None;
        UpdateCursorForDragState();
        pointer?.Capture(null);
        _activePointer = null;
        InvalidateVisual();
    }

    private void UpdateCursorLocation(Point position)
    {
        CursorPosition = Provider.Projection.PixelsToWgs84(
            GetMapPixel(position),
            Zoom,
            Provider.TileSize
        );
    }

    private void DragMapCenter(Point delta)
    {
        var centerPixel = Provider.Projection.Wgs84ToPixels(CenterMap, Zoom, Provider.TileSize);
        var mapDelta = RotateVector(delta, -Rotation);
        CenterMap = Provider.Projection.PixelsToWgs84(
            centerPixel - mapDelta,
            Zoom,
            Provider.TileSize
        );
    }

    private void UpdateSelectRectangle(Point position)
    {
        SelectionWidth = Math.Abs(_startMousePosition.X - position.X);
        SelectionHeight = Math.Abs(_startMousePosition.Y - position.Y);
        SelectionLeft = Math.Min(_startMousePosition.X, position.X);
        SelectionTop = Math.Min(_startMousePosition.Y, position.Y);

        var rect = new Rect(SelectionLeft, SelectionTop, SelectionWidth, SelectionHeight);
        Selection.BeginBatchUpdate();
        try
        {
            Selection.Clear();
            foreach (var item in Items)
            {
                if (item is null)
                {
                    continue;
                }

                if (ContainerFromItem(item) is not MapItem control)
                {
                    continue;
                }

                if (!rect.Intersects(control.Bounds))
                {
                    continue;
                }

                control.IsSelected = true;
                var index = IndexFromContainer(control);
                Selection.Select(index);
            }
        }
        finally
        {
            Selection.EndBatchUpdate();
        }
    }

    private void DragSelectedItems(Point delta)
    {
        var mapDelta = RotateVector(delta, -Rotation);

        foreach (var item in Selection.SelectedItems)
        {
            if (item == null)
            {
                continue;
            }

            if (ContainerFromItem(item) is not MapItem ctrl || ctrl.IsReadOnly)
            {
                continue;
            }

            var location = ctrl.Location;
            var currentPixel = Provider.Projection.Wgs84ToPixels(location, Zoom, Provider.TileSize);
            var newLocation = Provider.Projection.PixelsToWgs84(
                currentPixel + mapDelta,
                Zoom,
                Provider.TileSize
            );
            ctrl.Location = newLocation;
        }
    }

    private Point GetMapPixel(Point screenPoint)
    {
        var centerScreen = new Point(Bounds.Width * 0.5, Bounds.Height * 0.5);
        var centerPixel = Provider.Projection.Wgs84ToPixels(CenterMap, Zoom, Provider.TileSize);
        return centerPixel + RotateVector(screenPoint - centerScreen, -Rotation);
    }

    private static Point RotateVector(Point vector, double angle)
    {
        if (angle == 0)
        {
            return vector;
        }

        var radians = angle * Math.PI / 180.0;
        var cosTheta = Math.Cos(radians);
        var sinTheta = Math.Sin(radians);

        return new Point(
            (vector.X * cosTheta) - (vector.Y * sinTheta),
            (vector.X * sinTheta) + (vector.Y * cosTheta)
        );
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        UpdateCursorLocation(e.GetPosition(this));

        if (EnableTouchpadGestures && IsLikelyTouchpadScroll(e))
        {
            e.Handled = true;
            return;
        }

        if (!EnableWheelZoom)
        {
            return;
        }

        if (Selection.SelectedItem != null)
        {
            CenterMap =
                (
                    Selection.SelectedItem as MapItem
                    ?? ContainerFromItem(Selection.SelectedItem) as MapItem
                )?.Location ?? CenterMap;
        }

        if (TryApplyZoomStepFromDelta(e.Delta.Y))
        {
            e.Handled = true;
        }
    }

    private bool IsLikelyTouchpadScroll(PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return false;
        }

        var absoluteX = Math.Abs(e.Delta.X);
        var absoluteY = Math.Abs(e.Delta.Y);

        if (absoluteX > 0.0)
        {
            return true;
        }

        return absoluteY > 0.0 && absoluteY < WheelDiscreteStepThreshold;
    }

    private void OnTouchPadGestureSwipe(object? sender, PointerDeltaEventArgs e)
    {
        if (!EnableTouchpadGestures)
        {
            return;
        }

        // Swipe gestures are intentionally ignored to avoid map pan on touchpad scroll.
        e.Handled = true;
    }

    private void OnTouchPadGestureMagnify(object? sender, PointerDeltaEventArgs e)
    {
        if (!EnableTouchpadGestures)
        {
            return;
        }

        var threshold = Math.Max(0.01, TouchpadMagnifyStepThreshold);
        _touchpadZoomAccumulator += e.Delta.Y * TouchpadZoomSensitivity;

        var zoomSteps = 0;
        while (_touchpadZoomAccumulator >= threshold)
        {
            zoomSteps++;
            _touchpadZoomAccumulator -= threshold;
        }

        while (_touchpadZoomAccumulator <= -threshold)
        {
            zoomSteps--;
            _touchpadZoomAccumulator += threshold;
        }

        if (TryApplyZoomSteps(zoomSteps))
        {
            e.Handled = true;
        }
    }

    private bool TryApplyZoomStepFromDelta(double delta)
    {
        if (delta > 0)
        {
            return TryApplyZoomSteps(1);
        }

        if (delta < 0)
        {
            return TryApplyZoomSteps(-1);
        }

        return false;
    }

    private bool TryApplyZoomSteps(int steps)
    {
        if (steps == 0)
        {
            return false;
        }

        var newZoom = Math.Clamp(_zoom + steps, MinZoom, MaxZoom);
        if (newZoom == _zoom)
        {
            return false;
        }

        Zoom = newZoom;
        return true;
    }

    private void UpdateCursorForDragState()
    {
        Cursor = DragState switch
        {
            DragState.DragMap => _sizeAllCursor ??= new Cursor(StandardCursorType.SizeAll),
            DragState.DragSelection => _handCursor ??= new Cursor(StandardCursorType.Hand),
            DragState.SelectRectangle => _crossCursor ??= new Cursor(StandardCursorType.Cross),
            _ => Cursor.Default,
        };
    }

    private static void DisposeAndResetCursor(ref Cursor? cursor)
    {
        cursor?.Dispose();
        cursor = null;
    }

    #endregion

    #region Dispose

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _activePointer?.Capture(null);
        _activePointer = null;
        _selectedContainer = null;
        DragState = DragState.None;
        Cursor = Cursor.Default;

        // Ensure native cursor handles are released when control leaves visual tree.
        DisposeAndResetCursor(ref _handCursor);
        DisposeAndResetCursor(ref _sizeAllCursor);
        DisposeAndResetCursor(ref _crossCursor);
    }

    #endregion
}
