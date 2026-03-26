using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
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
            CenterMapProperty,
            RotationProperty
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
            context.FillRectangle(background, new Rect(Bounds.Size));
        }

        if (Provider is null)
        {
            return;
        }

        var tileSize = Provider.TileSize;
        var zoom = Zoom;
        var tilesCount = 1 << zoom;

        var centerPixel = Provider.Projection.Wgs84ToPixels(CenterMap, zoom, tileSize);
        var offset = new Point(
            (Bounds.Width / 2.0) - centerPixel.X,
            (Bounds.Height / 2.0) - centerPixel.Y
        );

        var renderRadius =
            Math.Sqrt((Bounds.Width * Bounds.Width) + (Bounds.Height * Bounds.Height)) / 2.0;
        var renderSize = renderRadius * 2.0;

        var tilesX = (int)Math.Ceiling(renderSize / tileSize) + 2;
        var tilesY = (int)Math.Ceiling(renderSize / tileSize) + 2;

        var centerScreenX = Bounds.Width / 2.0;
        var centerScreenY = Bounds.Height / 2.0;

        var startX = (int)Math.Floor((centerScreenX - renderRadius - offset.X) / tileSize) - 1;
        var startY = (int)Math.Floor((centerScreenY - renderRadius - offset.Y) / tileSize) - 1;

        // Сдвигаем координаты так, чтобы центр оказался в (0,0)
        var matrix = Matrix.CreateTranslation(-centerScreenX, -centerScreenY);
        matrix *= Matrix.CreateRotation(Rotation * Math.PI / 180.0);
        matrix *= Matrix.CreateTranslation(centerScreenX, centerScreenY);

        using (context.PushTransform(matrix))
        {
            for (var x = startX; x < startX + tilesX; x++)
            {
                if (x < 0 || x >= tilesCount)
                {
                    continue;
                }

                for (var y = startY; y < startY + tilesY; y++)
                {
                    if (y < 0 || y >= tilesCount)
                    {
                        continue;
                    }

                    var key = new TileKey(x, y, zoom, Provider);

                    var px = (key.X * tileSize) + offset.X;
                    var py = (key.Y * tileSize) + offset.Y;

                    _tileLoader.Render(context, px, py, key);

                    if (IsDebug)
                    {
                        context.DrawRectangle(
                            Brushes.Transparent,
                            new Pen(Brushes.Red),
                            new Rect(px, py, tileSize, tileSize)
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
                var center = Provider.Projection.Wgs84ToPixels(CenterMap, zoom, tileSize) + offset;

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
    }

    private void RequestRenderLoop() => _renderRequestSubject.OnNext(Unit.Default);

    #endregion
}
