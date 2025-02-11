using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.Map;

public partial class MapControl : TemplatedControl
{
    public const int TileSize = 256;
    private Point _offset;
    private Point _lastMousePosition;
    private ITileLoader _cache;
    private readonly Subject<Unit> _renderRequestSubject = new();
    private readonly IDisposable _disposeIt;

    public MapControl()
    {
        Zoom = 8;
        DisposableBuilder disposeBuilder = new();
        _renderRequestSubject.AddTo(ref disposeBuilder);
        _renderRequestSubject
            .ThrottleLastFrame(1)
            .Subscribe(_ => InvalidateVisual())
            .AddTo(ref disposeBuilder);

        Observable
            .FromEventHandler<PointerPressedEventArgs>(
                x => PointerPressed += x,
                x => PointerPressed -= x
            )
            .Subscribe(OnPointerPressed)
            .AddTo(ref disposeBuilder);

        Observable
            .FromEventHandler<PointerReleasedEventArgs>(
                x => PointerReleased += x,
                x => PointerReleased -= x
            )
            .Subscribe(OnPointerReleased)
            .AddTo(ref disposeBuilder);

        _provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
        _cache = new CacheTileLoader(MapCore.LoggerFactory, _provider);
        Disposable.Create(() => _cache?.Dispose());

        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
        _cache.OnLoaded.Subscribe(x => _renderRequestSubject.OnNext(Unit.Default));
        _disposeIt = disposeBuilder.Build();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _disposeIt.Dispose();
        base.OnUnloaded(e);
    }

    #region Events

    private void OnPointerPressed((object? sender, PointerPressedEventArgs e) args)
    {
        _lastMousePosition = args.e.GetPosition(this);
    }

    private void OnPointerReleased((object? sender, PointerReleasedEventArgs e) args) { }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var currentPosition = e.GetPosition(this);
        CursorPosition = Provider.Projection.PixelsToWgs84(
            currentPosition - _offset,
            Zoom,
            Provider.TileSize
        );
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _offset += currentPosition - _lastMousePosition;
            _lastMousePosition = currentPosition;
            CenterMap = Provider.Projection.PixelsToWgs84(
                new Point(Bounds.Width / 2 - _offset.X, Bounds.Height / 2 - _offset.Y),
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
        var center = Provider.Projection.Wgs84ToPixels(CenterMap, newZoom, Provider.TileSize);
        _offset = new Point(Bounds.Width / 2, Bounds.Height / 2) - center;
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
            Background,
            null,
            new Rect(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height)
        );

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

                var px = (key.X * TileSize) + _offset.X;
                var py = (key.Y * TileSize) + _offset.Y;
                var tile = _cache[key];
                context.DrawImage(
                    tile,
                    new Rect(0, 0, TileSize, TileSize),
                    new Rect(px, py, TileSize, TileSize)
                );
                if (IsDebug)
                {
                    context.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red),
                        new Rect(px, py, TileSize, TileSize)
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

    private void RequestRenderLoop() => _renderRequestSubject?.OnNext(Unit.Default);
}
