using System.Diagnostics;
using System.Globalization;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.Map;

public class MapBackground : Control
{
    private ITileLoader? _cache;
    private readonly Subject<Unit> _renderRequestSubject = new();
    private readonly IDisposable _disposeIt;

    static MapBackground()
    {
        AffectsRender<MapBackground>(BackgroundProperty);
    }

    public MapBackground()
    {
        IsDebug = true;
        DisposableBuilder disposeBuilder = new();
        _renderRequestSubject.AddTo(ref disposeBuilder);
        _renderRequestSubject
            .ThrottleLastFrame(1)
            .Subscribe(_ => InvalidateVisual())
            .AddTo(ref disposeBuilder);

        Provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
        Disposable.Create(() => _cache?.Dispose());

        _disposeIt = disposeBuilder.Build();

        Zoom = 8;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _disposeIt.Dispose();
        base.OnUnloaded(e);
    }

    #region Render

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        Debug.Assert(_cache != null, "_cache != null");

        var background = Background;
        if (background != null)
        {
            var renderSize = Bounds.Size;
            context.FillRectangle(background, new Rect(renderSize));
        }

        var tileSize = Provider.TileSize;
        var zoom = Zoom;
        var tiles = 1 << zoom;

        var tilesX = (int)Math.Ceiling(Bounds.Width / tileSize) + 2;
        var tilesY = (int)Math.Ceiling(Bounds.Height / tileSize) + 2;

        var startX = (int)-_offset.X / tileSize - 1;
        var startY = (int)-_offset.Y / tileSize - 1;

        for (var x = startX; x < startX + tilesX; x++)
        {
            if (x < 0 || x > tiles)
            {
                continue;
            }

            for (var y = startY; y < startY + tilesY; y++)
            {
                if (y < 0 || y > tiles)
                {
                    continue;
                }
                var key = new TilePosition(x, y, zoom);

                var px = (key.X * Provider.TileSize) + _offset.X;
                var py = (key.Y * Provider.TileSize) + _offset.Y;
                var tile = _cache[key];
                context.DrawImage(
                    tile,
                    new Rect(0, 0, Provider.TileSize, Provider.TileSize),
                    new Rect(px, py, Provider.TileSize, Provider.TileSize)
                );
                if (IsDebug)
                {
                    context.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red),
                        new Rect(px, py, Provider.TileSize, Provider.TileSize)
                    );
                    context.DrawText(
                        new FormattedText(
                            $"{x},{y}[{px:F0},{py:F0}]",
                            CultureInfo.CurrentUICulture,
                            FlowDirection.LeftToRight,
                            Typeface.Default,
                            12.0,
                            Brushes.Violet
                        ),
                        new Point(px, py)
                    );
                }
            }
        }

        if (IsDebug)
        {
            var center = Provider.Projection.Wgs84ToPixels(_centerMap, zoom, tileSize) + _offset;
            context.DrawLine(
                new Pen(Brushes.Red, 2),
                new Point(center.X - 25, center.Y),
                new Point(center.X + 25, center.Y)
            );
            context.DrawLine(
                new Pen(Brushes.Red, 2),
                new Point(center.X, center.Y - 25),
                new Point(center.X, center.Y + 25)
            );
        }
    }

    private void RequestRenderLoop() => _renderRequestSubject.OnNext(Unit.Default);

    #endregion


    #region Offset

    private Point _offset;

    public static readonly DirectProperty<MapBackground, Point> OffsetProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, Point>(
            nameof(Offset),
            o => o.Offset,
            (o, v) => o.Offset = v
        );

    public Point Offset
    {
        get => _offset;
        set
        {
            if (SetAndRaise(OffsetProperty, ref _offset, value))
            {
                RequestRenderLoop();
            }
        }
    }

    #endregion

    #region CenterMap

    private GeoPoint _centerMap;

    public static readonly DirectProperty<MapBackground, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, GeoPoint>(
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
                var centerPixel = Provider.Projection.Wgs84ToPixels(value, Zoom, Provider.TileSize);
                Offset = new Point(
                    Bounds.Width / 2 - centerPixel.X,
                    Bounds.Height / 2 - centerPixel.Y
                );
            }
        }
    }

    #endregion

    #region Provider

    private ITileProvider _provider = EmptyTileProvider.Instance;
    public static readonly DirectProperty<MapBackground, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v,
            unsetValue: EmptyTileProvider.Instance
        );

    public ITileProvider Provider
    {
        get => _provider;
        set
        {
            if (SetAndRaise(ProviderProperty, ref _provider, value))
            {
                ProviderChanged(_provider);
            }
        }
    }

    private void ProviderChanged(ITileProvider provider)
    {
        _cache?.Dispose();
        _cache = new CacheTileLoader(MapCore.LoggerFactory, provider ?? EmptyTileProvider.Instance);
        _cache.OnLoaded.Subscribe(x => _renderRequestSubject.OnNext(Unit.Default));
        RequestRenderLoop();
    }

    #endregion

    #region Zoom

    private int _zoom;

    public static readonly DirectProperty<MapBackground, int> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, int>(
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
                var center = Provider.Projection.Wgs84ToPixels(CenterMap, value, Provider.TileSize);
                Offset = new Point(Bounds.Width / 2, Bounds.Height / 2) - center;
            }
        }
    }

    #endregion

    #region IsDebug

    private bool _isDebug;

    public static readonly DirectProperty<MapBackground, bool> IsDebugEnabledProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, bool>(
            nameof(IsDebug),
            o => o.IsDebug,
            (o, v) => o.IsDebug = v
        );

    public bool IsDebug
    {
        get => _isDebug;
        set
        {
            if (SetAndRaise(IsDebugEnabledProperty, ref _isDebug, value))
            {
                RequestRenderLoop();
            }
        }
    }

    #endregion

    #region Background

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<MapBackground>();

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    #endregion
}
