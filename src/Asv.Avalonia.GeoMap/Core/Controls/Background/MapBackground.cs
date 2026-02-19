using System.Globalization;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.GeoMap;

public partial class MapBackground : Control
{
    private readonly Subject<Unit> _renderRequestSubject = new();
    private readonly IDisposable _disposeIt;
    private readonly ITileLoader _tileLoader;

    static MapBackground()
    {
        AffectsRender<MapBackground>(
            BackgroundProperty,
            ZoomProperty,
            ProviderProperty,
            CenterMapProperty
        );
    }

    public MapBackground()
    {
        IsDebug = true;
        _tileLoader = AppHost.Instance.Services.GetRequiredService<ITileLoader>();

        DisposableBuilder disposeBuilder = new();
        _renderRequestSubject.AddTo(ref disposeBuilder);
        _renderRequestSubject
            .ThrottleLastFrame(1)
            .Subscribe(_ => InvalidateVisual())
            .AddTo(ref disposeBuilder);
        _tileLoader.OnLoaded.Subscribe(_ => RequestRenderLoop()).AddTo(ref disposeBuilder);

        Provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
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

        var background = Background;
        if (background != null)
        {
            var renderSize = Bounds.Size;
            context.FillRectangle(background, new Rect(renderSize));
        }

        var centerPixel = Provider.Projection.Wgs84ToPixels(CenterMap, Zoom, Provider.TileSize);
        var offset = new Point(
            (Bounds.Width / 2) - centerPixel.X,
            (Bounds.Height / 2) - centerPixel.Y
        );
        var tileSize = Provider.TileSize;
        var zoom = Zoom;
        var tiles = 1 << zoom;

        var tilesX = (int)Math.Ceiling(Bounds.Width / tileSize) + 2;
        var tilesY = (int)Math.Ceiling(Bounds.Height / tileSize) + 2;

        var startX = ((int)-offset.X / tileSize) - 1;
        var startY = ((int)-offset.Y / tileSize) - 1;

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

                var key = new TileKey(x, y, zoom, Provider);

                var px = (key.X * Provider.TileSize) + offset.X;
                var py = (key.Y * Provider.TileSize) + offset.Y;
                var tile = _tileLoader[key];
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
            var center = Provider.Projection.Wgs84ToPixels(_centerMap, zoom, tileSize) + offset;
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
