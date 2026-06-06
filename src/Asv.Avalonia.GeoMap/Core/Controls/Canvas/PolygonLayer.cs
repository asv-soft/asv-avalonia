using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public partial class PolygonLayer : Control
{
    private readonly Dictionary<MapItem, StreamGeometry> _geometryCache = new();
    private readonly Dictionary<MapItem, INotifyCollectionChanged> _trackedPolygons = new();
    private readonly HashSet<MapItem> _trackedContainers = new();
    private readonly Subject<Unit> _renderRequestSubject = new();
    private MapItemsControl? _attachedSource;

    static PolygonLayer()
    {
        SourceProperty
            .Changed.ToObservable()
            .Subscribe(e =>
            {
                if (e.Sender is PolygonLayer layer)
                {
                    layer.AttachSource(e.NewValue.HasValue ? e.NewValue.Value : null);
                }
            });
    }

    public PolygonLayer()
    {
        _renderRequestSubject.ThrottleLastFrame(1).Subscribe(_ => InvalidateVisual());
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AttachSource(Source);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        DetachSource();
        ClearGeometryCache();
        base.OnUnloaded(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
        {
            InvalidateGeometryCache();
        }
    }

    private void AttachSource(MapItemsControl? source)
    {
        if (ReferenceEquals(_attachedSource, source))
        {
            return;
        }

        DetachSource();
        _attachedSource = source;

        if (source == null)
        {
            InvalidateGeometryCache();
            return;
        }

        source.PropertyChanged += SourcePropertyChanged;
        source.ContainerPrepared += SourceContainerPrepared;
        source.ContainerClearing += SourceContainerClearing;

        foreach (var sourceItem in source.GetRealizedContainers())
        {
            if (sourceItem is MapItem container)
            {
                TrackContainer(container);
            }
        }

        InvalidateGeometryCache();
    }

    private void DetachSource()
    {
        if (_attachedSource != null)
        {
            _attachedSource.PropertyChanged -= SourcePropertyChanged;
            _attachedSource.ContainerPrepared -= SourceContainerPrepared;
            _attachedSource.ContainerClearing -= SourceContainerClearing;
        }

        foreach (var container in _trackedContainers.ToArray())
        {
            UntrackContainer(container);
        }

        _attachedSource = null;
    }

    private void SourceContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        var container = e.Container as MapItem;
        if (container == null)
        {
            Debug.Assert(false, nameof(container) + " != null");
            return;
        }

        UntrackContainer(container);
        InvalidateGeometry(container);
    }

    private void SourceContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        var container = e.Container as MapItem;
        if (container == null)
        {
            Debug.Assert(false, nameof(container) + " != null");
            return;
        }

        TrackContainer(container);
        InvalidateGeometry(container);
    }

    private void TrackContainer(MapItem container)
    {
        if (!_trackedContainers.Add(container))
        {
            return;
        }

        container.PropertyChanged += ContainerPropertyChanged;
        TrackPolygon(container);
    }

    private void UntrackContainer(MapItem container)
    {
        if (!_trackedContainers.Remove(container))
        {
            return;
        }

        container.PropertyChanged -= ContainerPropertyChanged;
        UntrackPolygon(container);
    }

    private void TrackPolygon(MapItem container)
    {
        if (container.Polygon is not INotifyCollectionChanged collection)
        {
            return;
        }

        collection.CollectionChanged += PolygonCollectionChanged;
        _trackedPolygons[container] = collection;
    }

    private void UntrackPolygon(MapItem container)
    {
        if (!_trackedPolygons.Remove(container, out var collection))
        {
            return;
        }

        collection.CollectionChanged -= PolygonCollectionChanged;
    }

    private void SourcePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (
            e.Property == MapItemsControl.ZoomProperty
            || e.Property == MapItemsControl.CenterMapProperty
            || e.Property == MapItemsControl.ProviderProperty
            || e.Property == MapItemsControl.RotationProperty
            || e.Property == BoundsProperty
        )
        {
            InvalidateGeometryCache();
        }
    }

    private void ContainerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not MapItem container)
        {
            return;
        }

        if (e.Property == MapItem.PolygonProperty)
        {
            UntrackPolygon(container);
            TrackPolygon(container);
            InvalidateGeometry(container);
            return;
        }

        if (e.Property == MapItem.IsPolygonClosedProperty)
        {
            InvalidateGeometry(container);
            return;
        }

        if (e.Property == MapItem.PenProperty || e.Property == MapItem.FillProperty)
        {
            _renderRequestSubject.OnNext(Unit.Default);
        }
    }

    private void PolygonCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is INotifyCollectionChanged collection)
        {
            foreach (var pair in _trackedPolygons)
            {
                if (ReferenceEquals(pair.Value, collection))
                {
                    InvalidateGeometry(pair.Key);
                    return;
                }
            }
        }

        InvalidateGeometryCache();
    }

    private void InvalidateGeometry(MapItem container)
    {
        _geometryCache.Remove(container);
        _renderRequestSubject.OnNext(Unit.Default);
    }

    private void InvalidateGeometryCache()
    {
        ClearGeometryCache();
        _renderRequestSubject.OnNext(Unit.Default);
    }

    private void ClearGeometryCache()
    {
        _geometryCache.Clear();
    }

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
        var state = new GeometryBuildState(projection, zoom, tileSize, offset, Source.Rotation);

        foreach (var sourceItem in Source.GetRealizedContainers())
        {
            if (sourceItem is not MapItem child)
            {
                continue;
            }

            if (child.Polygon is not { Count: > 1 })
            {
                continue;
            }

            var geometry = GetOrCreateGeometry(child, state);
            context.DrawGeometry(child.Fill, child.Pen, geometry);
        }
    }

    private StreamGeometry GetOrCreateGeometry(MapItem item, GeometryBuildState state)
    {
        if (_geometryCache.TryGetValue(item, out var geometry))
        {
            return geometry;
        }

        geometry = CreateGeometry(item, state);
        _geometryCache[item] = geometry;
        return geometry;
    }

    private StreamGeometry CreateGeometry(MapItem item, GeometryBuildState state)
    {
        var geometry = new StreamGeometry();
        var polygon = item.Polygon;
        Debug.Assert(polygon is { Count: > 1 }, "Polygon geometry requires at least two points.");

        using (var ctx = geometry.Open())
        {
            var start = RotatePoint(
                state.Projection.Wgs84ToPixels(polygon[0], state.Zoom, state.TileSize)
                    + state.Offset,
                state.Rotation
            );
            ctx.BeginFigure(start, item.IsPolygonClosed);

            foreach (var point in polygon)
            {
                var nextPoint = RotatePoint(
                    state.Projection.Wgs84ToPixels(point, state.Zoom, state.TileSize)
                        + state.Offset,
                    state.Rotation
                );
                ctx.LineTo(nextPoint);
            }

            if (item.IsPolygonClosed)
            {
                ctx.LineTo(start);
            }
        }

        return geometry;
    }

    private Point RotatePoint(Point point, double angle)
    {
        var radians = angle * Math.PI / 180.0;
        var cosTheta = Math.Cos(radians);
        var sinTheta = Math.Sin(radians);

        var dx = point.X - (Bounds.Width / 2.0);
        var dy = point.Y - (Bounds.Height / 2.0);

        return new Point(
            (dx * cosTheta) - (dy * sinTheta) + (Bounds.Width / 2.0),
            (dx * sinTheta) + (dy * cosTheta) + (Bounds.Height / 2.0)
        );
    }

    private readonly struct GeometryBuildState
    {
        public GeometryBuildState(
            IMapProjection projection,
            int zoom,
            int tileSize,
            Point offset,
            double rotation
        )
        {
            Projection = projection;
            Zoom = zoom;
            TileSize = tileSize;
            Offset = offset;
            Rotation = rotation;
        }

        public IMapProjection Projection { get; }
        public int Zoom { get; }
        public int TileSize { get; }
        public Point Offset { get; }
        public double Rotation { get; }
    }
}
