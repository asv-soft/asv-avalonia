using System.Collections;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Metadata;

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
    private Point _lastMousePosition;
    private Point _startMousePosition;
    private Control? _selectedContainer;

    public MapItemsControl()
    {
        SelectionMode = SelectionMode.Multiple;
        SelectionChanged += OnSelectionChanged;
    }

    #region Property events

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MinZoomProperty || change.Property == MaxZoomProperty)
        {
            Zoom = Math.Clamp(Zoom, MinZoom, MaxZoom);
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
        _startMousePosition = _lastMousePosition = e.GetPosition(this);
        if (e.KeyModifiers == KeyModifiers.Shift)
        {
            UpdateSelectRectangle(_startMousePosition);
            DragState = DragState.SelectRectangle;
        }
        else
        {
            var container = GetContainerFromEventSource(e.Source);
            if (container != null)
            {
                DragState = DragState.DragSelection;
                _selectedContainer = container;
                var index = IndexFromContainer(container);
                if (Selection.IsSelected(index) == false)
                {
                    Selection.Clear();
                }

                Selection.Select(index);
            }
            else
            {
                DragState = DragState.DragMap;
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var position = e.GetPosition(this);
        var delta = position - _lastMousePosition;

        UpdateCursorLocation(position);

        if (Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y)) < 5)
        {
            return;
        }

        _selectedContainer = null;
        switch (DragState)
        {
            case DragState.SelectRectangle:
                UpdateSelectRectangle(position);
                InvalidateVisual();
                break;
            case DragState.DragSelection:
                Cursor = new Cursor(StandardCursorType.Hand);
                DragSelectedItems(delta);
                break;
            case DragState.DragMap:
                Cursor = new Cursor(StandardCursorType.SizeAll);
                DragMapCenter(delta);
                break;
            case DragState.None:
            default:
                break;
        }

        _lastMousePosition = position;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        ClearDragState();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        ClearDragState();
    }

    private void ClearDragState()
    {
        if (_selectedContainer != null)
        {
            Selection.BeginBatchUpdate();
            Selection.Clear();
            Selection.Select(IndexFromContainer(_selectedContainer));
            Selection.EndBatchUpdate();
        }

        DragState = DragState.None;
        Cursor = Cursor.Default;
        InvalidateVisual();
    }

    private void UpdateSelectRectangle(Point position)
    {
        SelectionWidth = Math.Abs(_startMousePosition.X - position.X);
        SelectionHeight = Math.Abs(_startMousePosition.Y - position.Y);
        SelectionLeft = SelectionLeft > position.X ? position.X : _startMousePosition.X;
        SelectionTop = SelectionTop > position.Y ? position.Y : _startMousePosition.Y;
        var rect = new Rect(SelectionLeft, SelectionTop, SelectionWidth, SelectionHeight);
        Selection.BeginBatchUpdate();
        Selection.Clear();
        foreach (var item in Items)
        {
            if (item == null)
            {
                continue;
            }

            var control = ContainerFromItem(item) as MapItem;
            if (control == null)
            {
                return;
            }

            if (rect.Intersects(control.Bounds))
            {
                control.IsSelected = true;
                var index = IndexFromContainer(control);
                Selection.Select(index);
            }
        }

        Selection.EndBatchUpdate();
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

    private void DragSelectedItems(Point delta)
    {
        var mapDelta = RotateVector(delta, -Rotation);

        foreach (var item in Selection.SelectedItems)
        {
            if (item == null)
            {
                continue;
            }

            if (ContainerFromItem(item) is MapItem ctrl)
            {
                if (ctrl.IsReadOnly)
                {
                    continue;
                }

                var location = ctrl.Location;
                var currentPixel = Provider.Projection.Wgs84ToPixels(
                    location,
                    Zoom,
                    Provider.TileSize
                );
                var newLocation = Provider.Projection.PixelsToWgs84(
                    currentPixel + mapDelta,
                    Zoom,
                    Provider.TileSize
                );
                ctrl.Location = newLocation;
            }
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
        var newZoom = _zoom;
        if (Selection.SelectedItem != null)
        {
            CenterMap =
                (
                    Selection.SelectedItem as MapItem
                    ?? ContainerFromItem(Selection.SelectedItem) as MapItem
                )?.Location ?? CenterMap;
        }

        if (e.Delta.Y > 0 && _zoom < MaxZoom)
        {
            newZoom++;
        }
        else if (e.Delta.Y < 0 && _zoom > MinZoom)
        {
            newZoom--;
        }

        if (newZoom != _zoom)
        {
            Zoom = newZoom;
        }
    }

    #endregion
}
