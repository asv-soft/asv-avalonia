using System.Collections.Specialized;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Asv.Avalonia.GeoMap;

/// <summary>Draws screen-vertical altitude stems independently of map rotation.</summary>
public sealed class AltitudeLayer : Control
{
    public static readonly StyledProperty<MapItemsControl?> SourceProperty =
        AvaloniaProperty.Register<AltitudeLayer, MapItemsControl?>(nameof(Source));
    public static readonly StyledProperty<bool> ShowPolygonAltitudesProperty =
        AvaloniaProperty.Register<AltitudeLayer, bool>(nameof(ShowPolygonAltitudes), true);
    public static readonly StyledProperty<double> PixelsPerMeterProperty =
        AvaloniaProperty.Register<AltitudeLayer, double>(nameof(PixelsPerMeter), 1,
            validate: value => double.IsFinite(value) && value > 0);
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<AltitudeLayer, IBrush?>(nameof(Stroke));

    private MapItemsControl? _attached;
    private readonly HashSet<MapItem> _items = [];
    private readonly HashSet<INotifyCollectionChanged> _polygons = [];
    public MapItemsControl? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public bool ShowPolygonAltitudes { get => GetValue(ShowPolygonAltitudesProperty); set => SetValue(ShowPolygonAltitudesProperty, value); }
    public double PixelsPerMeter { get => GetValue(PixelsPerMeterProperty); set => SetValue(PixelsPerMeterProperty, value); }
    public IBrush? Stroke { get => GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }

    public AltitudeLayer() { IsHitTestVisible = false; ClipToBounds = true; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Reconnect();
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Disconnect();
        base.OnDetachedFromVisualTree(e);
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty || change.Property == IsVisibleProperty
            || change.Property == ShowPolygonAltitudesProperty)
        {
            Reconnect();
        }

        if (IsVisible)
        {
            InvalidateVisual();
        }
    }
    private void Disconnect()
    {
        if (_attached is not null)
        {
            _attached.PropertyChanged -= MapChanged;
            _attached.ContainerPrepared -= Prepared;
            _attached.ContainerClearing -= Clearing;
            _attached.LayoutUpdated -= LayoutChanged;
        }
        foreach (var item in _items)
        {
            item.PropertyChanged -= ItemChanged;
        }

        foreach (var polygon in _polygons)
        {
            polygon.CollectionChanged -= PolygonChanged;
        }

        _items.Clear();
        _polygons.Clear();
        _attached = null;
    }
    private void Reconnect()
    {
        Disconnect();
        if (!IsVisible || !this.IsAttachedToVisualTree() || Source is null)
        {
            return;
        }

        _attached = Source;
        _attached.PropertyChanged += MapChanged;
        _attached.ContainerPrepared += Prepared;
        _attached.ContainerClearing += Clearing;
        _attached.LayoutUpdated += LayoutChanged;
        TrackItems();
        InvalidateVisual();
    }
    private void TrackItems()
    {
        foreach (var item in _items)
        {
            item.PropertyChanged -= ItemChanged;
        }

        foreach (var polygon in _polygons)
        {
            polygon.CollectionChanged -= PolygonChanged;
        }

        _items.Clear();
        _polygons.Clear();
        if (_attached is null)
        {
            return;
        }

        foreach (var item in _attached.GetRealizedContainers().OfType<MapItem>())
        {
            _items.Add(item);
            item.PropertyChanged += ItemChanged;
            if (ShowPolygonAltitudes && item.Polygon is INotifyCollectionChanged polygon && _polygons.Add(polygon))
            {
                polygon.CollectionChanged += PolygonChanged;
            }
        }
    }
    private void MapChanged(object? sender, AvaloniaPropertyChangedEventArgs e) => InvalidateVisual();
    private void LayoutChanged(object? sender, EventArgs e) => InvalidateVisual();
    private void Prepared(object? sender, ContainerPreparedEventArgs e) { TrackItems(); InvalidateVisual(); }
    private void Clearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container is MapItem item && _items.Remove(item))
        {
            item.PropertyChanged -= ItemChanged;
        }

        // Retain subscriptions only for polygons belonging to remaining containers.
        foreach (var polygon in _polygons)
        {
            polygon.CollectionChanged -= PolygonChanged;
        }

        _polygons.Clear();
        foreach (var remaining in _items)
            if (ShowPolygonAltitudes && remaining.Polygon is INotifyCollectionChanged polygon && _polygons.Add(polygon))
            {
                polygon.CollectionChanged += PolygonChanged;
            }

        InvalidateVisual();
    }
    private void ItemChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MapItem.PolygonProperty)
        {
            TrackItems();
        }

        InvalidateVisual();
    }
    private void PolygonChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateVisual();

    public override void Render(DrawingContext context)
    {
        if (!IsVisible || _attached is null || Stroke is null)
        {
            return;
        }

        foreach (var item in _items)
        {
            if (!item.IsVisible || !ShowPolygonAltitudes || item.Polygon is not { Count: > 0 } polygon)
            {
                continue;
            }

            var pen = item.Pen ?? new Pen(Stroke, 1.5);
            var vertices = polygon.Select(vertex =>
            {
                var bottom = Project(vertex);
                var height = Math.Abs(vertex.Altitude) * PixelsPerMeter;
                return (Bottom: bottom, Top: bottom - new Point(0, height), Valid: double.IsFinite(height));
            }).ToArray();
            var edgeCount = item.IsPolygonClosed && vertices.Length > 2
                ? vertices.Length
                : vertices.Length - 1;

            // Fill all side faces before drawing outlines to retain clear edges.
            using (context.PushOpacity(0.15))
            {
                for (var i = 0; i < edgeCount; i++)
                {
                    var a = vertices[i];
                    var b = vertices[(i + 1) % vertices.Length];
                    if (!a.Valid || !b.Valid)
                    {
                        continue;
                    }

                    var face = new StreamGeometry();
                    using (var geometry = face.Open())
                    {
                        geometry.BeginFigure(a.Bottom, true);
                        geometry.LineTo(a.Top);
                        geometry.LineTo(b.Top);
                        geometry.LineTo(b.Bottom);
                        geometry.EndFigure(true);
                    }

                    context.DrawGeometry(pen.Brush, null, face);
                }
            }

            using (context.PushOpacity(0.5))
            {
                foreach (var vertex in vertices)
                {
                    if (vertex.Valid)
                    {
                        context.DrawLine(pen, vertex.Bottom, vertex.Top);
                    }
                }

                for (var i = 0; i < edgeCount; i++)
                {
                    var a = vertices[i];
                    var b = vertices[(i + 1) % vertices.Length];
                    if (a.Valid && b.Valid)
                    {
                        context.DrawLine(pen, a.Top, b.Top);
                    }
                }
            }
        }
    }

    private Point Project(GeoPoint location)
    {
        var source = _attached!;
        var half = new Point(source.Bounds.Width / 2, source.Bounds.Height / 2);
        var projection = source.Provider.Projection;
        var offset = projection.Wgs84ToPixels(location, source.Zoom, source.Provider.TileSize)
            - projection.Wgs84ToPixels(source.CenterMap, source.Zoom, source.Provider.TileSize);
        var angle = source.Rotation * Math.PI / 180;
        var point = half + new Point((offset.X * Math.Cos(angle)) - (offset.Y * Math.Sin(angle)),
            (offset.X * Math.Sin(angle)) + (offset.Y * Math.Cos(angle)));
        return source.TranslatePoint(point, this) ?? point;
    }
}
