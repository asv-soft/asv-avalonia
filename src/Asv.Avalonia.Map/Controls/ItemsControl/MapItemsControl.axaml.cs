using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Asv.Avalonia.Map;

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
        Provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
        SelectionMode = SelectionMode.Multiple;
        SelectionChanged += OnSelectionChanged;
    }

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
                Selection.Select(IndexFromContainer(container));
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

        var centerScreen = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var offset =
            centerScreen - Provider.Projection.Wgs84ToPixels(CenterMap, Zoom, Provider.TileSize);

        UpdateCursorLocation(offset, position);

        if (Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y) < 5)
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
                DragMapCenter(offset, delta, centerScreen);
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
                continue;
            var control = ContainerFromItem(item) as MapItem;
            if (control == null)
                return;

            if (rect.Intersects(control.Bounds))
            {
                control.IsSelected = true;
                var index = IndexFromContainer(control);
                Selection.Select(index);
            }
        }
        Selection.EndBatchUpdate();
    }

    private void UpdateCursorLocation(Point offset, Point position)
    {
        CursorPosition = Provider.Projection.PixelsToWgs84(
            position - offset,
            Zoom,
            Provider.TileSize
        );
    }

    private void DragMapCenter(Point offset, Point delta, Point center)
    {
        CenterMap = Provider.Projection.PixelsToWgs84(
            center - offset - delta,
            Zoom,
            Provider.TileSize
        );
    }

    private void DragSelectedItems(Point delta)
    {
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
                var newPixelX = currentPixel.X + delta.X;
                var newPixelY = currentPixel.Y + delta.Y;
                var newLocation = Provider.Projection.PixelsToWgs84(
                    new Point(newPixelX, newPixelY),
                    Zoom,
                    Provider.TileSize
                );
                ctrl.Location = newLocation;
            }
        }
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
        if (e.Delta.Y > 0 && _zoom < 19)
        {
            newZoom++;
        }
        else if (e.Delta.Y < 0 && _zoom > 1)
        {
            newZoom--;
        }
        if (newZoom != _zoom)
        {
            Zoom = newZoom;
        }
    }
}
