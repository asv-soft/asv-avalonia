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
            e.OldValue.Value.ItemsView.CollectionChanged -= SourceCollectionChanged;
        }

        if (e.NewValue is { HasValue: true, Value: not null })
        {
            e.NewValue.Value.PropertyChanged += SourcePropertyChanged;
            e.NewValue.Value.ItemsView.CollectionChanged += SourceCollectionChanged;
        }
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
    }

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Debug.Assert(Source != null, nameof(Source) + " != null");
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                var container = item as MapItem ?? Source.ContainerFromItem(item) as MapItem;
                if (container?.Polygon == null)
                {
                    continue;
                }

                if (container?.Polygon is INotifyCollectionChanged coll)
                {
                    coll.CollectionChanged += PolygonCollectionChanged;
                }

                _renderRequestSubject.OnNext(Unit.Default);
            }
        }

        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                var container = item as MapItem ?? Source.ContainerFromItem(item) as MapItem;
                if (container?.Polygon == null)
                {
                    continue;
                }

                if (container?.Polygon is INotifyCollectionChanged coll)
                {
                    coll.CollectionChanged -= PolygonCollectionChanged;
                }

                _renderRequestSubject.OnNext(Unit.Default);
            }
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
        Debug.WriteLine("Plygon render");
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
                var start = projection.Wgs84ToPixels(polygon[0], zoom, tileSize) + offset;
                ctx.BeginFigure(start, child.IsPolygonClosed);
                foreach (var point in child.Polygon)
                {
                    var nextPoint = projection.Wgs84ToPixels(point, zoom, tileSize) + offset;
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
}
