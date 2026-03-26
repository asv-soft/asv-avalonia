using System.Collections.Specialized;
using System.Diagnostics;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public partial class AnnotationLayer : Canvas
{
    static AnnotationLayer()
    {
        SourceProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is AnnotationLayer layer)
            {
                layer.MapControlSourceUpdated(e);
            }
        });
    }

    private readonly List<MapAnnotation> _annotations = new();
    private readonly Subject<Unit> _renderRequestSubject = new();

    public AnnotationLayer()
    {
        // TODO: when dispose it?
        _renderRequestSubject.ThrottleLastFrame(1).Subscribe(_ => UpdateAnnotationsFromChildren());
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
    }

    private void MapControlSourceUpdated(AvaloniaPropertyChangedEventArgs<MapItemsControl?> e)
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
        container.PropertyChanged -= ContainerOnPropertyChanged;
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
        container.PropertyChanged += ContainerOnPropertyChanged;
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

    private void ContainerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MapItem.LocationProperty)
        {
            _renderRequestSubject.OnNext(Unit.Default);
        }
    }

    private void UpdateAnnotationsFromChildren()
    {
        if (Source == null || ItemTemplate == null)
        {
            return;
        }

        _annotations.RemoveAll(a =>
        {
            if (Source.GetRealizedContainers().Contains(a.Target))
            {
                return false;
            }

            a.Dispose();
            return true;
        });

        foreach (var sourceItem in Source.GetRealizedContainers())
        {
            if (sourceItem is not MapItem child)
            {
                continue;
            }

            var location = child.Location;

            var existingAnnotation = _annotations.FirstOrDefault(a => a.Target == child);
            if (existingAnnotation == null)
            {
                var content = ItemTemplate.Build(child.DataContext ?? child);
                if (content == null)
                {
                    continue;
                }

                content.DataContext = child.DataContext ?? child;

                var annotation = content;
                var connector = new Line { Stroke = Stroke, StrokeThickness = StrokeThickness };

                var anchorPos = ConvertToScreen(location);
                var initialDirection = GetInitialDirection(_annotations.Count);
                var initialOffset = initialDirection * AnnotationRadius;

                var item = new MapAnnotation(child, annotation, connector)
                {
                    AnchorPoint = location,
                    ScreenPosition = anchorPos + initialOffset,
                };

                _annotations.Add(item);
                Children.Add(connector);
                Children.Add(annotation);
            }
            else
            {
                existingAnnotation.AnchorPoint = location;
            }
        }

        ArrangeAnnotations(Bounds.Size);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (Source == null)
        {
            return;
        }

        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var position = e.GetPosition(this);
        var hitAnnotation = _annotations.FirstOrDefault(a =>
            a.Annotation.Bounds.Contains(position) || a.Target.Bounds.Contains(position)
        );

        if (hitAnnotation == null)
        {
            return;
        }

        // Toggle the IsSelected state of the Target
        // hitAnnotation.Target.IsSelected = true;
        Source.Selection.Clear();
        Source.Selection.Select(Source.IndexFromContainer(hitAnnotation.Target));
        hitAnnotation.Target.Focus();
        UpdateVisuals(); // Redraw to display the change
        e.Handled = true;
    }

    private Point GetInitialDirection(int index)
    {
        // Distribute annotations around a circle
        double angle = 2 * Math.PI * index / 8; // 8 is the approximate number of directions
        return new Point(Math.Cos(angle), Math.Sin(angle));
    }

    private Point ConvertToScreen(GeoPoint geoPoint)
    {
        if (Source == null)
        {
            return new Point(0, 0);
        }

        var tileSize = Source.Provider.TileSize;
        var projection = Source.Provider.Projection;
        var centerScreen = new Point(Source.Bounds.Width * 0.5, Source.Bounds.Height * 0.5);
        var centerPixel = projection.Wgs84ToPixels(Source.CenterMap, Source.Zoom, tileSize);
        var offset = centerScreen - centerPixel;
        var screenPoint = projection.Wgs84ToPixels(geoPoint, Source.Zoom, tileSize) + offset;

        return Source.Rotation == 0
            ? screenPoint
            : RotatePoint(screenPoint, centerScreen, Source.Rotation);
    }

    private void UpdateVisuals()
    {
        foreach (var item in _annotations)
        {
            // Center the TextBlock relative to the ScreenPosition
            var textWidth = item.Annotation.Bounds.Width;
            var textHeight = item.Annotation.Bounds.Height;
            SetLeft(item.Annotation, item.ScreenPosition.X - (textWidth / 2));
            SetTop(item.Annotation, item.ScreenPosition.Y - (textHeight / 2));

            var anchorPos = ConvertToScreen(item.AnchorPoint);
            item.Connector.StartPoint = anchorPos;
            item.Connector.EndPoint = item.ScreenPosition;
        }
    }

    public void ArrangeAnnotations(Size finalSize)
    {
        if (Source == null || _annotations.Count == 0)
        {
            return;
        }

        Debug.WriteLine("ArrangeAnnotations");
        const int baseMaxIterations = 100;
        const double repulsionStrength = 1000.0;
        const double attractionStrength = 0.1;
        const double damping = 0.9;
        double minDistance = AnnotationRadius;
        const double maxVelocity = 10.0; // Velocity limit to prevent jerky movement
        const double stabilizationThreshold = 0.1; // Stabilization threshold (in pixels)

        // Adaptive iteration count: reduce for a large number of annotations
        int maxIterations = Math.Min(baseMaxIterations, 1000 / Math.Max(1, _annotations.Count));

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            bool stabilized = true;

            foreach (var item in _annotations)
            {
                var currentPos = item.ScreenPosition;
                var anchorPos = ConvertToScreen(item.AnchorPoint);
                var velocity = new Point(0, 0);

                // Attraction to the preferred radial position
                var preferredDirection = (currentPos - anchorPos).Normalize();
                var preferredPos = anchorPos + (preferredDirection * minDistance);
                velocity += (preferredPos - currentPos) * attractionStrength;

                // Repulsion from other annotations with optimization
                foreach (var other in _annotations)
                {
                    if (other == item)
                    {
                        continue;
                    }

                    var delta = currentPos - other.ScreenPosition;
                    var distanceSquared = delta.LengthSquared(); // Faster than Length

                    if (!(distanceSquared < minDistance * minDistance))
                    {
                        continue; // Check squared distance
                    }

                    var distance = Math.Sqrt(distanceSquared);
                    var repulsion =
                        delta.Normalize() * (repulsionStrength / Math.Max(distance, 1.0));
                    velocity += repulsion;
                    stabilized = false;
                }

                // Limit velocity to avoid excessive jumps
                var velocityMagnitude = velocity.Length();
                if (velocityMagnitude > maxVelocity)
                {
                    velocity = velocity.Normalize() * maxVelocity;
                }

                // Update position
                var newPos = currentPos + (velocity * damping);
                newPos = new Point(
                    Math.Clamp(newPos.X, 0, finalSize.Width - item.Annotation.Bounds.Width),
                    Math.Clamp(newPos.Y, 0, finalSize.Height - item.Annotation.Bounds.Height)
                );

                // Check if the movement is small enough for stabilization
                var movement = (newPos - currentPos).Length();
                if (movement > stabilizationThreshold)
                {
                    stabilized = false;
                }

                item.ScreenPosition = newPos;
            }

            if (stabilized)
            {
                break;
            }
        }

        UpdateVisuals();
    }

    private static Point RotatePoint(Point point, Point center, double angle)
    {
        var radians = angle * Math.PI / 180.0;
        var cosTheta = Math.Cos(radians);
        var sinTheta = Math.Sin(radians);
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;

        return new Point(
            (dx * cosTheta) - (dy * sinTheta) + center.X,
            (dx * sinTheta) + (dy * cosTheta) + center.Y
        );
    }
}

public class MapAnnotation : AsyncDisposableOnce
{
    private readonly IDisposable _dispose;

    public MapAnnotation(MapItem target, Control annotation, Line connector)
    {
        Target = target;
        Annotation = annotation;
        Connector = connector;
        target
            .ObservePropertyChanged(x => x.IsAnnotationVisible)
            .Subscribe(x =>
            {
                Connector.IsVisible = x;
                Annotation.IsVisible = x;
            });
        Connector.IsVisible = Target.IsAnnotationVisible;
        Annotation.IsVisible = Target.IsAnnotationVisible;

        _dispose = target
            .ObservePropertyChanged(x => x.IsSelected)
            .Subscribe(x =>
            {
                Connector.Classes.Set("active", x);
                Annotation.Classes.Set("active", x);
            });
    }

    public MapItem Target { get; }
    public Control Annotation { get; }
    public Line Connector { get; }

    public GeoPoint AnchorPoint { get; set; }
    public Point ScreenPosition { get; set; }

    protected override void Dispose(bool disposing)
    {
        _dispose.Dispose();
        base.Dispose(disposing);
    }
}
