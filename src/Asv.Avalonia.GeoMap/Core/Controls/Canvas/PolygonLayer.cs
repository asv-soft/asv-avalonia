using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public partial class PolygonLayer : Control
{
    static PolygonLayer()
    {
        SourceProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is PolygonLayer layer)
            {
                layer.SourceUpdated(e);
            }
        });
    }

    private void SourceUpdated(AvaloniaPropertyChangedEventArgs<MapItemsControl?> e)
    {
        if (e.OldValue is { HasValue: true, Value: not null })
        {
            e.OldValue.Value.PropertyChanged -= SourcePropertyChanged;
            e.OldValue.Value.ContainerPrepared -= SourceContainerPrepared;
            e.OldValue.Value.ContainerClearing -= SourceContainerClearing;
        }

        if (e.NewValue is { HasValue: true, Value: not null })
        {
            e.NewValue.Value.PropertyChanged += SourcePropertyChanged;
            e.NewValue.Value.ContainerPrepared += SourceContainerPrepared;
            e.NewValue.Value.ContainerClearing += SourceContainerClearing;
        }
    }

    private void SourceContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        var container = e.Container as MapItem;
        if (container == null)
        {
            Debug.Assert(false, nameof(container) + " != null");
            return;
        }
        if (container?.Polygon is INotifyCollectionChanged coll)
        {
            coll.CollectionChanged -= PolygonCollectionChanged;
        }

        _renderRequestSubject.OnNext(Unit.Default);
    }

    private void SourceContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        var container = e.Container as MapItem;
        if (container == null)
        {
            Debug.Assert(false, nameof(container) + " != null");
            return;
        }

        if (container?.Polygon is INotifyCollectionChanged coll)
        {
            coll.CollectionChanged += PolygonCollectionChanged;
        }

        _renderRequestSubject.OnNext(Unit.Default);
    }

    private void SourcePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MapItemsControl.ZoomProperty)
        {
            _renderRequestSubject.OnNext(Unit.Default);
        }

        if (e.Property == MapItemsControl.CenterMapProperty)
        {
            _renderRequestSubject.OnNext(Unit.Default);
        }

        if (e.Property == MapItemsControl.ProviderProperty)
        {
            _renderRequestSubject.OnNext(Unit.Default);
        }

        if (e.Property == MapItemsControl.RotationProperty)
        {
            _renderRequestSubject.OnNext(Unit.Default);
        }
    }

    public PolygonLayer()
    {
        // TODO: when disposing?
        _renderRequestSubject.ThrottleLastFrame(1).Subscribe(_ => InvalidateVisual());
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
    }

    private void PolygonCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _renderRequestSubject.OnNext(Unit.Default);
    }

    private readonly Subject<Unit> _renderRequestSubject = new();

    public override void Render(DrawingContext context)
    {
        if (Source == null)
        {
            return;
        }

        var tileSize = Source.Provider.TileSize;
        var halfWidth = Source.Bounds.Width * 0.5;
        var halfHeight = Source.Bounds.Height * 0.5;
        var projection = Source.Provider.Projection;
        var zoom = Source.Zoom;
        var centerPixel = projection.Wgs84ToPixels(Source.CenterMap, zoom, tileSize);
        var offset = new Point(halfWidth - centerPixel.X, halfHeight - centerPixel.Y);
        Debug.WriteLine("Polygon render");

        var rotationAngle = Source.Rotation; // Получаем угол поворота карты

        foreach (var sourceItem in Source.GetRealizedContainers())
        {
            var child = sourceItem as MapItem;
            if (child == null)
            {
                continue;
            }

            if (child.Polygon is not { Count: > 1 })
            {
                continue;
            }

            var geometry = new StreamGeometry();

            var polygon = child.Polygon;
            using (var ctx = geometry.Open())
            {
                // Начальная точка полигона
                var start = RotatePoint(
                    projection.Wgs84ToPixels(polygon[0], zoom, tileSize) + offset,
                    rotationAngle
                );
                ctx.BeginFigure(start, child.IsPolygonClosed);

                // Рисуем все остальные точки с учётом поворота
                foreach (var point in polygon)
                {
                    var nextPoint = RotatePoint(
                        projection.Wgs84ToPixels(point, zoom, tileSize) + offset,
                        rotationAngle
                    );
                    ctx.LineTo(nextPoint);
                }

                if (child.IsPolygonClosed)
                {
                    ctx.LineTo(start);
                }
            }

            context.DrawGeometry(child.Fill, child.Pen, geometry);
        }
    }

    // Функция для поворота точки вокруг центра
    private Point RotatePoint(Point point, double angle)
    {
        var radians = angle * Math.PI / 180.0; // Преобразуем угол в радианы
        var cosTheta = Math.Cos(radians);
        var sinTheta = Math.Sin(radians);

        // Смещение точки относительно центра
        var dx = point.X - (Bounds.Width / 2.0);
        var dy = point.Y - (Bounds.Height / 2.0);

        // Применяем поворот
        var rotatedX = (dx * cosTheta) - (dy * sinTheta) + (Bounds.Width / 2.0);
        var rotatedY = (dx * sinTheta) + (dy * cosTheta) + (Bounds.Height / 2.0);

        return new Point(rotatedX, rotatedY);
    }
}
