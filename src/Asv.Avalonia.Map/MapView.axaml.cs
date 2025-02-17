using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Asv.Avalonia.Map;

public class MapView : SelectingItemsControl
{
    private Point _lastMousePosition;

    public MapView()
    {
        Provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );

        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    #region Pointer Events

    private void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        _lastMousePosition = args.GetPosition(this);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs args) { }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var offset =
            new Point(Bounds.Width / 2, Bounds.Height / 2)
            - Provider.Projection.Wgs84ToPixels(CenterMap, Zoom, Provider.TileSize);

        var currentPosition = e.GetPosition(this);
        CursorPosition = Provider.Projection.PixelsToWgs84(
            currentPosition - offset,
            Zoom,
            Provider.TileSize
        );

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            offset = offset + currentPosition - _lastMousePosition;
            _lastMousePosition = currentPosition;
            CenterMap = Provider.Projection.PixelsToWgs84(
                new Point(Bounds.Width / 2 - offset.X, Bounds.Height / 2 - offset.Y),
                Zoom,
                Provider.TileSize
            );
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
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

    public static readonly DirectProperty<MapView, GeoPoint> CursorPositionProperty =
        AvaloniaProperty.RegisterDirect<MapView, GeoPoint>(
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

    public static readonly DirectProperty<MapView, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapView, GeoPoint>(
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
    public static readonly DirectProperty<MapView, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapView, ITileProvider>(
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

    public static readonly DirectProperty<MapView, int> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapView, int>(
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
