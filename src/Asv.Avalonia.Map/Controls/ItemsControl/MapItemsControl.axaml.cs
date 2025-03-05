using System.Collections;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Metadata;

namespace Asv.Avalonia.Map;

public enum DragState
{
    None,
    DragSelection,
    DragMap,
    SelectRectangle,
}

public class MapItemsControl : SelectingItemsControl
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

    #region ItemTemplate Property

    public static readonly StyledProperty<IDataTemplate?> AnnotationTemplateProperty =
        AvaloniaProperty.Register<MapItemsControl, IDataTemplate?>(nameof(AnnotationTemplate));

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? AnnotationTemplate
    {
        get => GetValue(AnnotationTemplateProperty);
        set => SetValue(AnnotationTemplateProperty, value);
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

    #region Selection rect

    private double _selectionLeft;

    public static readonly DirectProperty<MapItemsControl, double> SelectionLeftProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionLeft),
            o => o.SelectionLeft,
            (o, v) => o.SelectionLeft = v
        );

    public double SelectionLeft
    {
        get => _selectionLeft;
        set => SetAndRaise(SelectionLeftProperty, ref _selectionLeft, value);
    }

    private double _selectionTop;

    public static readonly DirectProperty<MapItemsControl, double> SelectionTopProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionTop),
            o => o.SelectionTop,
            (o, v) => o.SelectionTop = v
        );

    public double SelectionTop
    {
        get => _selectionTop;
        set => SetAndRaise(SelectionTopProperty, ref _selectionTop, value);
    }

    private double _selectionWidth;

    public static readonly DirectProperty<MapItemsControl, double> SelectionWidthProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionWidth),
            o => o.SelectionWidth,
            (o, v) => o.SelectionWidth = v
        );

    public double SelectionWidth
    {
        get => _selectionWidth;
        set => SetAndRaise(SelectionWidthProperty, ref _selectionWidth, value);
    }

    private double _selectionHeight;

    public static readonly DirectProperty<MapItemsControl, double> SelectionHeightProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionHeight),
            o => o.SelectionHeight,
            (o, v) => o.SelectionHeight = v
        );

    public double SelectionHeight
    {
        get => _selectionHeight;
        set => SetAndRaise(SelectionHeightProperty, ref _selectionHeight, value);
    }

    #endregion

    #region Drag state

    private DragState _dragState;

    public static readonly DirectProperty<MapItemsControl, DragState> DragStateProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, DragState>(
            nameof(DragState),
            o => o.DragState,
            (o, v) => o.DragState = v
        );

    public DragState DragState
    {
        get => _dragState;
        set => SetAndRaise(DragStateProperty, ref _dragState, value);
    }

    #endregion

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

    #endregion

    #region Cursor position

    private GeoPoint _cursorPosition;

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CursorPositionProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
            nameof(CursorPosition),
            o => o.CursorPosition,
            (o, v) => o.CursorPosition = v
        );

    public GeoPoint CursorPosition
    {
        get => _cursorPosition;
        set => SetAndRaise(CursorPositionProperty, ref _cursorPosition, value);
    }

    #endregion

    #region CenterMap

    private GeoPoint _centerMap;

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
            nameof(CenterMap),
            o => o.CenterMap,
            (o, v) => o.CenterMap = v
        );

    public GeoPoint CenterMap
    {
        get => _centerMap;
        set => SetAndRaise(CenterMapProperty, ref _centerMap, value);
    }

    #endregion

    #region Provider

    private ITileProvider _provider = EmptyTileProvider.Instance;
    public static readonly DirectProperty<MapItemsControl, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v
        );

    public ITileProvider Provider
    {
        get => _provider;
        set => SetAndRaise(ProviderProperty, ref _provider, value);
    }

    #endregion

    #region Zoom

    private int _zoom = 8;

    public static readonly DirectProperty<MapItemsControl, int> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, int>(
            nameof(Zoom),
            o => o.Zoom,
            (o, v) => o.Zoom = v
        );

    public int Zoom
    {
        get => _zoom;
        set => SetAndRaise(ZoomProperty, ref _zoom, value);
    }

    #endregion
}
