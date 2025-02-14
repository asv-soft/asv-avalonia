using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using R3;

namespace Asv.Avalonia.Map;

public partial class MapControl : SelectingItemsControl
{
    private Point _offset;
    private Point _lastMousePosition;
    private ITileLoader _cache;
    private readonly Subject<Unit> _renderRequestSubject = new();
    private readonly IDisposable _disposeIt;

    public MapControl()
    {
        IsDebug = true;
        Zoom = 8;
        DisposableBuilder disposeBuilder = new();
        _renderRequestSubject.AddTo(ref disposeBuilder);
        _renderRequestSubject
            .ThrottleLastFrame(1)
            .Subscribe(_ => InvalidateVisual())
            .AddTo(ref disposeBuilder);

        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;

        _provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
        _cache = new CacheTileLoader(MapCore.LoggerFactory, _provider);

        Disposable.Create(() => _cache?.Dispose());

        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
        LogicalChildren.CollectionChanged += (_, _) => _renderRequestSubject.OnNext(Unit.Default);
        _cache
            .OnLoaded.Subscribe(x => _renderRequestSubject.OnNext(Unit.Default))
            .AddTo(ref disposeBuilder);

        _disposeIt = disposeBuilder.Build();
    }

    /*protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new MapControlItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<MapControlItem>(item, out recycleKey);
    }*/

    #region Load

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _disposeIt.Dispose();
        base.OnUnloaded(e);
    }

    #endregion

    #region Pointer Events

    private void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        _lastMousePosition = args.GetPosition(this);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs args) { }

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
            var offset = _offset + currentPosition - _lastMousePosition;
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
        var center = Provider.Projection.Wgs84ToPixels(CenterMap, newZoom, Provider.TileSize);
        _offset = new Point(Bounds.Width / 2, Bounds.Height / 2) - center;
        if (newZoom != _zoom)
        {
            Zoom = newZoom;
        }
    }

    #endregion

    [Content]
    public Controls Children { get; } = new Controls();

    #region Render

    public override void Render(DrawingContext context)
    {
        RenderMapTiles(context);
        base.Render(context);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return base.ArrangeOverride(finalSize);
    }

    private void RenderChildren(DrawingContext context)
    {
        foreach (var child in LogicalChildren.OfType<Control>())
        {
            var location = GetLocation(child);
            if (location == null)
                continue;
            var pixelPos =
                Provider.Projection.Wgs84ToPixels(location.Value, Zoom, Provider.TileSize)
                + _offset;
            var rect = new Rect(
                pixelPos.X - child.Bounds.Width / 2,
                pixelPos.Y - child.Bounds.Height / 2,
                child.Bounds.Width,
                child.Bounds.Height
            );
            child.Render(context);
        }
    }

    private void RenderMapTiles(DrawingContext context)
    {
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
}
