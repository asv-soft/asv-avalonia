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
            Children.Remove(a.Annotation);
            Children.Remove(a.Connector);
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
            var anchorPos = ConvertToScreen(location);

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

                var initialDirection = GetInitialDirection(_annotations.Count);
                var initialOffset = initialDirection * AnnotationRadius;

                var item = new MapAnnotation(child, annotation, connector)
                {
                    AnchorPoint = location,
                    AnchorScreenPosition = anchorPos,
                    PreferredDirection = initialDirection,
                    ScreenOffset = initialOffset,
                    ScreenPosition = anchorPos + initialOffset,
                };

                _annotations.Add(item);
                Children.Add(connector);
                Children.Add(annotation);
            }
            else
            {
                existingAnnotation.AnchorPoint = location;
                existingAnnotation.AnchorScreenPosition = anchorPos;
                existingAnnotation.ScreenPosition = anchorPos + existingAnnotation.ScreenOffset;
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
            item.ScreenPosition = item.AnchorScreenPosition + item.ScreenOffset;

            // Center the TextBlock relative to the ScreenPosition
            var size = GetAnnotationSize(item.Annotation);
            var textWidth = size.Width;
            var textHeight = size.Height;
            SetLeft(item.Annotation, item.ScreenPosition.X - (textWidth / 2));
            SetTop(item.Annotation, item.ScreenPosition.Y - (textHeight / 2));

            item.Connector.StartPoint = item.AnchorScreenPosition;
            item.Connector.EndPoint = item.ScreenPosition;
        }
    }

    public void ArrangeAnnotations(Size finalSize)
    {
        if (Source == null || _annotations.Count == 0)
        {
            return;
        }

        const int maxIterations = 12;
        const double minVectorLength = 0.001;
        const double stabilizationThreshold = 0.1;
        const double collisionPadding = 8.0;
        const double boundaryStrength = 0.5;
        const double returnStrength = 0.15;
        const double maxStep = 24.0;
        const double maxFrameOffsetChange = 10.0;

        var currentOffsets = _annotations
            .Select(x => EnsureMinimumOffset(x.ScreenOffset, x.PreferredDirection))
            .ToArray();
        var anchorPositions = _annotations.Select(x => x.AnchorScreenPosition).ToArray();
        var preferredDirections = _annotations.Select(x => x.PreferredDirection).ToArray();
        var annotationSizes = _annotations.Select(x => GetAnnotationSize(x.Annotation)).ToArray();
        var forces = new Point[_annotations.Count];
        var nextOffsets = new Point[_annotations.Count];

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            bool stabilized = true;

            for (var index = 0; index < _annotations.Count; index++)
            {
                forces[index] = GetReturnForce(
                    currentOffsets[index],
                    preferredDirections[index],
                    AnnotationRadius,
                    returnStrength
                );
            }

            for (var index = 0; index < _annotations.Count; index++)
            {
                var currentPos = anchorPositions[index] + currentOffsets[index];
                var currentSize = annotationSizes[index];

                for (var otherIndex = index + 1; otherIndex < _annotations.Count; otherIndex++)
                {
                    var otherPos = anchorPositions[otherIndex] + currentOffsets[otherIndex];
                    var otherSize = annotationSizes[otherIndex];
                    if (
                        !TryGetSeparationVector(
                            currentPos,
                            currentSize,
                            otherPos,
                            otherSize,
                            preferredDirections[index],
                            preferredDirections[otherIndex],
                            collisionPadding,
                            out var separation
                        )
                    )
                    {
                        continue;
                    }

                    forces[index] += separation * 0.5;
                    forces[otherIndex] -= separation * 0.5;
                    stabilized = false;
                }
            }

            for (var index = 0; index < _annotations.Count; index++)
            {
                var step = forces[index];
                var stepMagnitude = step.Length();
                if (stepMagnitude > maxStep)
                {
                    step = step.Normalize() * maxStep;
                }

                var newOffset = currentOffsets[index] + step;
                newOffset = EnsureMinimumOffset(
                    newOffset,
                    preferredDirections[index],
                    minVectorLength
                );
                var clampedPosition = ClampToViewport(
                    anchorPositions[index] + newOffset,
                    annotationSizes[index],
                    finalSize
                );
                newOffset = clampedPosition - anchorPositions[index];

                var boundaryCorrection =
                    (clampedPosition - (anchorPositions[index] + currentOffsets[index]))
                    * boundaryStrength;
                if (boundaryCorrection.LengthSquared() > minVectorLength)
                {
                    newOffset = currentOffsets[index] + step + boundaryCorrection;
                    newOffset = EnsureMinimumOffset(
                        newOffset,
                        preferredDirections[index],
                        minVectorLength
                    );
                    clampedPosition = ClampToViewport(
                        anchorPositions[index] + newOffset,
                        annotationSizes[index],
                        finalSize
                    );
                    newOffset = clampedPosition - anchorPositions[index];
                }

                var movement = (newOffset - currentOffsets[index]).Length();
                if (movement > stabilizationThreshold)
                {
                    stabilized = false;
                }

                nextOffsets[index] = newOffset;
            }

            for (var index = 0; index < _annotations.Count; index++)
            {
                currentOffsets[index] = nextOffsets[index];
            }

            if (stabilized)
            {
                break;
            }
        }

        for (var index = 0; index < _annotations.Count; index++)
        {
            var item = _annotations[index];
            var offset = EnsureMinimumOffset(
                currentOffsets[index],
                preferredDirections[index],
                minVectorLength
            );
            offset = LimitOffsetChange(item.ScreenOffset, offset, maxFrameOffsetChange);
            item.ScreenOffset = offset;
            item.ScreenPosition = anchorPositions[index] + offset;
        }

        UpdateVisuals();
    }

    private Size GetAnnotationSize(Control annotation)
    {
        var width = annotation.Bounds.Width;
        var height = annotation.Bounds.Height;

        if (width <= 0 || height <= 0)
        {
            annotation.Measure(Size.Infinity);
            width = Math.Max(width, annotation.DesiredSize.Width);
            height = Math.Max(height, annotation.DesiredSize.Height);
        }

        return new Size(Math.Max(1.0, width), Math.Max(1.0, height));
    }

    private Point ClampToViewport(Point center, Size size, Size viewport)
    {
        var halfWidth = size.Width * 0.5;
        var halfHeight = size.Height * 0.5;
        var minX = Math.Min(halfWidth, viewport.Width * 0.5);
        var maxX = Math.Max(minX, viewport.Width - halfWidth);
        var minY = Math.Min(halfHeight, viewport.Height * 0.5);
        var maxY = Math.Max(minY, viewport.Height - halfHeight);

        return new Point(Math.Clamp(center.X, minX, maxX), Math.Clamp(center.Y, minY, maxY));
    }

    private Point EnsureMinimumOffset(
        Point offset,
        Point fallbackDirection,
        double minVectorLength = 0.001
    )
    {
        var direction = GetDirection(offset, fallbackDirection, minVectorLength);
        var distance = Math.Max(Math.Sqrt(offset.LengthSquared()), AnnotationRadius);
        return direction * distance;
    }

    private static Point GetDirection(Point offset, Point fallbackDirection, double minVectorLength)
    {
        return offset.LengthSquared() > minVectorLength ? offset.Normalize() : fallbackDirection;
    }

    private static Point GetReturnForce(
        Point offset,
        Point preferredDirection,
        double targetDistance,
        double returnStrength
    )
    {
        var targetOffset = preferredDirection * targetDistance;
        return (targetOffset - offset) * returnStrength;
    }

    private static Point LimitOffsetChange(Point currentOffset, Point nextOffset, double maxChange)
    {
        var delta = nextOffset - currentOffset;
        var distance = delta.Length();
        if (distance <= maxChange)
        {
            return nextOffset;
        }

        return currentOffset + (delta.Normalize() * maxChange);
    }

    private static bool TryGetSeparationVector(
        Point center1,
        Size size1,
        Point center2,
        Size size2,
        Point fallbackDirection1,
        Point fallbackDirection2,
        double padding,
        out Point separation
    )
    {
        const double tieEpsilon = 1.0;

        var dx = center1.X - center2.X;
        var dy = center1.Y - center2.Y;
        var overlapX = ((size1.Width + size2.Width) * 0.5) + padding - Math.Abs(dx);
        if (overlapX <= 0)
        {
            separation = default;
            return false;
        }

        var overlapY = ((size1.Height + size2.Height) * 0.5) + padding - Math.Abs(dy);
        if (overlapY <= 0)
        {
            separation = default;
            return false;
        }

        if (overlapX < overlapY)
        {
            var sign =
                Math.Abs(dx) < tieEpsilon
                    ? Math.Sign(fallbackDirection1.X - fallbackDirection2.X)
                    : Math.Sign(dx);
            if (sign == 0)
            {
                sign = 1;
            }

            separation = new Point(sign * overlapX, 0);
            return true;
        }

        var ySign =
            Math.Abs(dy) < tieEpsilon
                ? Math.Sign(fallbackDirection1.Y - fallbackDirection2.Y)
                : Math.Sign(dy);
        if (ySign == 0)
        {
            ySign = 1;
        }

        separation = new Point(0, ySign * overlapY);
        return true;
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
    public Point AnchorScreenPosition { get; set; }
    public Point PreferredDirection { get; set; }
    public Point ScreenOffset { get; set; }
    public Point ScreenPosition { get; set; }

    protected override void Dispose(bool disposing)
    {
        _dispose.Dispose();
        base.Dispose(disposing);
    }
}
