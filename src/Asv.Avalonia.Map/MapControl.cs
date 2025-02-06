using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Asv.Avalonia.Map;

public partial class MapControl : Control
{
    public const int TileSize = 256;

    private readonly Dictionary<TileKey, Bitmap?> _tiles = new();
    private Point _offset;
    private Point _lastMousePosition;

    public MapControl()
    {
        Zoom = 1;
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
        CenterMap = new GeoPoint(55.7558, 37.6173, 0);
    }

    #region Events

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastMousePosition = e.GetPosition(this);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            Point currentPosition = e.GetPosition(this);
            _offset += currentPosition - _lastMousePosition;
            _lastMousePosition = currentPosition;
            RecalculateCenterOffset();
            UpdateCenterTile();
            LoadVisibleTiles();
            InvalidateVisual();
        }
    }

    private int _centerTileX;
    private int _centerTileY;

    private void UpdateCenterTile()
    {
        int newCenterX = (int)((-(_offset.X - (Bounds.Width / 2))) / TileSize);
        int newCenterY = (int)((-(_offset.Y - (Bounds.Height / 2))) / TileSize);

        if (_centerTileX != newCenterX || _centerTileY != newCenterY)
        {
            _centerTileX = newCenterX;
            _centerTileY = newCenterY;
            LoadVisibleTiles();
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        int newZoom = _zoom;

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
        foreach (var (key, bitmap) in _tiles)
        {
            if (bitmap != null)
            {
                var px = (key.X * TileSize) + _offset.X;
                var py = (key.Y * TileSize) + _offset.Y;

                if (
                    px + TileSize < 0
                    || py + TileSize < 0
                    || px > Bounds.Width
                    || py > Bounds.Height
                )
                {
                    continue;
                }

                context.DrawImage(
                    bitmap,
                    new Rect(0, 0, TileSize, TileSize),
                    new Rect(px, py, TileSize, TileSize)
                );
            }
        }
    }

    private void RecalculateCenterOffset()
    {
        var tileSize = Provider.TileSize;
        var key = TileKey.FromGeoPoint(CenterMap, tileSize, Zoom);
        _offset = new Point(
            -(key.X * tileSize) + (Bounds.Width / 2),
            -(key.Y * tileSize) + (Bounds.Height / 2)
        );
    }

    private async void LoadVisibleTiles()
    {
        var tileSize = Provider.TileSize;
        var provider = Provider;
        var zoom = Zoom;
        int tilesX = (int)Math.Ceiling(Bounds.Width / tileSize) + 2;
        int tilesY = (int)Math.Ceiling(Bounds.Height / tileSize) + 2;

        var center = TileKey.FromGeoPoint(CenterMap, tileSize, Zoom);

        int startX = center.X - (tilesX / 2);
        int startY = center.Y - (tilesY / 2);

        var newTiles = new List<Task>();

        for (int x = startX; x < startX + tilesX; x++)
        {
            for (int y = startY; y < startY + tilesY; y++)
            {
                var key = new TileKey(x, y, zoom);
                if (!_tiles.ContainsKey(key))
                {
                    newTiles.Add(LoadTileAsync(key, provider));
                }
            }
        }

        await Task.WhenAll(newTiles);
        InvalidateVisual();
    }

    private async Task LoadTileAsync(TileKey key, ITileProvider provider)
    {
        var tile = await _cache.GetTileAsync(key, provider, CancellationToken.None);
        _tiles[key] = tile;
    }

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
                RecalculateCenterOffset();
                LoadVisibleTiles();
            }
        }
    }

    #endregion

    #region Provider

    private ITileProvider _provider = new BingTileProvider(
        "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
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
        set => SetAndRaise(ProviderProperty, ref _provider, value);
    }

    #endregion

    #region Cache

    private ITileLoader _cache = new OnlineTileLoader(MapCore.LoggerFactory);

    public static readonly DirectProperty<MapControl, ITileLoader> CacheProperty =
        AvaloniaProperty.RegisterDirect<MapControl, ITileLoader>(
            nameof(Cache),
            o => o.Cache,
            (o, v) => o.Cache = v
        );

    public ITileLoader Cache
    {
        get => _cache;
        set => SetAndRaise(CacheProperty, ref _cache, value);
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
                RecalculateCenterOffset();
                LoadVisibleTiles();
            }
        }
    }

    #endregion
}
