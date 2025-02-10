using System.Diagnostics;
using System.Globalization;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.Map;

public partial class MapControl : Control
{
    public const int TileSize = 256;
    private Point _offset;
    private Point _lastMousePosition;
    private readonly Subject<Unit> _renderSubject = new();
    private readonly IDisposable _sub1;

    public MapControl()
    {
        Zoom = 8;
        _sub1 = Observable
            .FromEventHandler<PointerPressedEventArgs>(
                x => PointerPressed += x,
                x => PointerPressed -= x
            )
            .Subscribe(e => OnPointerPressed(e.sender, e.e));
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
        //CenterMap = new GeoPoint(55.7558, 37.6173, 0);
        _cache.OnLoaded.Subscribe(x => _renderSubject.OnNext(Unit.Default));
        _renderSubject.ThrottleLastFrame(1).Subscribe(x => InvalidateVisual());
    }

    #region Events

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastMousePosition = e.GetPosition(this);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var currentPosition = e.GetPosition(this);
        CursorPosition = GetGeoPointFromCursor(currentPosition);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _offset += currentPosition - _lastMousePosition;
            Debug.WriteLine($"{_offset}");
            _lastMousePosition = currentPosition;
            _centerMap = GetGeoPointFromCursor(new Point(Bounds.Width / 2, Bounds.Height / 2));
            _renderSubject.OnNext(Unit.Default);
        }
    }

    private GeoPoint GetGeoPointFromCursor(Point cursorPosition)
    {
        // Получаем глобальные пиксельные координаты с учетом смещения
        var globalPx = cursorPosition.X - _offset.X;
        var globalPy = cursorPosition.Y - _offset.Y;

        // Рассчитываем размер всей карты в пикселях на текущем уровне Zoom
        var mapSize = TileSize * (1 << Zoom); // 256 * 2^Zoom

        // Обрабатываем переполнение по X (долгота)
        if (globalPx < 0)
            globalPx += mapSize;
        if (globalPx >= mapSize)
            globalPx -= mapSize;

        if (globalPy < 0)
            globalPy += mapSize;
        if (globalPy >= mapSize)
            globalPy -= mapSize;

        // Нормализуем координаты (0-1)
        var xNorm = (double)globalPx / mapSize;
        var yNorm = (double)globalPy / mapSize;

        // Преобразуем в долготу (lon), учитывая, что карта циклическая
        var lon = xNorm * 360.0 - 180.0;

        // Преобразуем в широту (lat) с использованием гиперболического синуса
        var latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * yNorm)));
        var lat = latRad * (180.0 / Math.PI);

        return new GeoPoint(lat, lon, 0);
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

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(
            Brushes.LightGray,
            null,
            new Rect(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height)
        );

        var tileSize = Provider.TileSize;

        var tilesX = (int)Math.Ceiling(Bounds.Width / tileSize) + 2;
        var tilesY = (int)Math.Ceiling(Bounds.Height / tileSize) + 2;

        var zoom = Zoom;
        var center = TilePosition.FromGeoPoint(CenterMap, tileSize, zoom);

        var startX = center.X - (tilesX / 2);
        var startY = center.Y - (tilesY / 2);

        for (var x = startX; x < startX + tilesX; x++)
        {
            for (var y = startY; y < startY + tilesY; y++)
            {
                var key = new TilePosition(x, y, zoom);
                if (x < 0 || y < 0)
                {
                    //Debug.WriteLine($"{x} {y}");
                }
                var px = (key.X * TileSize) + _offset.X;
                var py = (key.Y * TileSize) + _offset.Y;
                /*
                if (
                    px + TileSize < 0
                    || py + TileSize < 0
                    || px > Bounds.Width
                    || py > Bounds.Height
                )
                {
                    continue;
                }
                */

                var tile = _cache[key];
                context.DrawImage(
                    tile,
                    new Rect(0, 0, TileSize, TileSize),
                    new Rect(px, py, TileSize, TileSize)
                );
            }
        }

        context.DrawText(
            new FormattedText(
                $"{CursorPosition} {_offset}",
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                12.0,
                Brushes.Violet
            ),
            new Point(0, 0)
        );
    }

    private void CenterMapChanged()
    {
        var tileSize = Provider.TileSize;
        var key = TilePosition.FromGeoPoint(CenterMap, tileSize, Zoom);
        _offset = new Point(
            -(key.X * tileSize) + (Bounds.Width / 2),
            -(key.Y * tileSize) + (Bounds.Height / 2)
        );
        _renderSubject.OnNext(Unit.Default);
    }

    #region Cursor position

    private GeoPoint _cursorPosition;

    public static readonly DirectProperty<MapControl, GeoPoint> CursorPositionProperty =
        AvaloniaProperty.RegisterDirect<MapControl, GeoPoint>(
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

    public static readonly DirectProperty<MapControl, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapControl, GeoPoint>(
            nameof(CenterMap),
            o => o.CenterMap,
            (o, v) => o.CenterMap = v
        );

    public GeoPoint CenterMap
    {
        get => _centerMap;
        set
        {
            if (SetAndRaise(CenterMapProperty, ref _centerMap, value))
            {
                CenterMapChanged();
            }
        }
    }

    #endregion

    #region Provider

    private ITileProvider _provider = new BingTileProvider(
        "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
    );
    private ITileLoader _cache = new OnlineTileLoader(
        MapCore.LoggerFactory,
        new BingTileProvider("Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg")
    );

    public static readonly DirectProperty<MapControl, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapControl, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v
        );

    public ITileProvider Provider
    {
        get => _provider;
        set
        {
            if (SetAndRaise(ProviderProperty, ref _provider, value))
            {
                _cache = new OnlineTileLoader(MapCore.LoggerFactory, _provider);
            }
        }
    }

    #endregion

    #region Zoom

    private int _zoom;

    public static readonly DirectProperty<MapControl, int> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapControl, int>(
            nameof(Zoom),
            o => o.Zoom,
            (o, v) => o.Zoom = v
        );

    public int Zoom
    {
        get => _zoom;
        set
        {
            if (SetAndRaise(ZoomProperty, ref _zoom, value))
            {
                CenterMapChanged();
                _renderSubject.OnNext(Unit.Default);
            }
        }
    }

    #endregion
}
