using System.Diagnostics;
using System.Globalization;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.Map;

public partial class MapControl : Control
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
        _renderRequestSubject.ThrottleLastFrame(1).Subscribe(x => InvalidateVisual());
        
        // 
        Observable
            .FromEventHandler<PointerPressedEventArgs>(
                x => PointerPressed += x,
                x => PointerPressed -= x
            )
            .Subscribe(OnPointerPressed)
            .AddTo(ref disposeBuilder);
        
        
        _provider = new BingTileProvider("Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg");
        _cache = new CacheTileLoader(MapCore.LoggerFactory, _provider);
        // cause 
        Disposable.Create(()=> _cache?.Dispose());
        
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
        //CenterMap = new GeoPoint(55.7558, 37.6173, 0);
        
        _cache.OnLoaded.Subscribe(x => _renderRequestSubject.OnNext(Unit.Default));
        
        
        _disposeIt = disposeBuilder.Build();
    }

    #region Events

    private void OnPointerPressed((object? sender, PointerPressedEventArgs e) args)
    {
        _lastMousePosition = args.e.GetPosition(this);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var currentPosition = e.GetPosition(this);
        CursorPosition = Provider.Projection.PixelsToWgs84(currentPosition, Zoom, Provider.TileSize);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _offset += currentPosition - _lastMousePosition;
            _lastMousePosition = currentPosition;
            RequestRenderLoop();
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
    }

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
        
        var tilesX = (int)Math.Ceiling(Bounds.Width / tileSize) + 2;
        var tilesY = (int)Math.Ceiling(Bounds.Height / tileSize) + 2;
        

        
        var startX = (int)_offset.X / tileSize;
        var startY = (int)_offset.Y / tileSize;

        for (var x = startX; x < startX + tilesX; x++)
        {
            for (var y = startY; y < startY + tilesY; y++)
            {
                var key = new TilePosition(x, y, zoom);
                
                var px = (key.X * TileSize) + _offset.X;
                var py = (key.Y * TileSize) + _offset.Y;
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

    private void RequestRenderLoop() => _renderRequestSubject?.OnNext(Unit.Default);
}
