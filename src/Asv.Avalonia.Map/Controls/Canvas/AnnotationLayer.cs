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

namespace Asv.Avalonia.Map;

public class AnnotationLayer : Canvas
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
        DisposableBuilder disposeBuilder = new();
        _renderRequestSubject.AddTo(ref disposeBuilder);
        _renderRequestSubject
            .ThrottleLastFrame(1)
            .Subscribe(_ => UpdateAnnotationsFromChildren())
            .AddTo(ref disposeBuilder);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _renderRequestSubject.Dispose();
        _annotations.ForEach(a => a.Dispose());
        _annotations.Clear();
        base.OnUnloaded(e);
    }

    #region ItemTemplate

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<AnnotationLayer, IDataTemplate?>(nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    #endregion

    #region Connector line

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<AnnotationLayer, double>(
            nameof(StrokeThickness),
            defaultValue: 1
        );

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly StyledProperty<IBrush?> StrokeProperty = AvaloniaProperty.Register<
        AnnotationLayer,
        IBrush?
    >(nameof(Stroke), defaultValue: Brushes.Violet);

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    #endregion

    #region Source

    public static readonly StyledProperty<MapItemsControl?> SourceProperty =
        AvaloniaProperty.Register<AnnotationLayer, MapItemsControl?>(nameof(Source));

    public MapItemsControl? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    #endregion

    private void MapControlSourceUpdated(AvaloniaPropertyChangedEventArgs<MapItemsControl?> e)
    {
        if (e.OldValue is { HasValue: true, Value: not null })
        {
            e.OldValue.Value.PropertyChanged -= SourcePropertyChanged;
            e.OldValue.Value.ItemsView.CollectionChanged -= SourceChanged;
        }

        if (e.NewValue is { HasValue: true, Value: not null })
        {
            e.NewValue.Value.PropertyChanged += SourcePropertyChanged;
            e.NewValue.Value.ItemsView.CollectionChanged += SourceChanged;
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

    private void SourceChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Debug.Assert(Source != null, nameof(Source) + " != null");
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                var container = item as MapItem ?? Source.ContainerFromItem(item) as MapItem;
                if (container == null)
                {
                    continue;
                }
                container.PropertyChanged += ContainerOnPropertyChanged;
                _renderRequestSubject.OnNext(Unit.Default);
            }
        }
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                var container = item as MapItem ?? Source.ContainerFromItem(item) as MapItem;
                if (container == null)
                {
                    continue;
                }
                container.PropertyChanged -= ContainerOnPropertyChanged;
                _renderRequestSubject.OnNext(Unit.Default);
            }
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
            return;
        _annotations.RemoveAll(a =>
        {
            if (Source.GetRealizedContainers().Contains(a.Target) == false)
            {
                a.Dispose();
                return true;
            }
            ;
            return false;
        });

        foreach (var sourceItem in Source.GetRealizedContainers())
        {
            var child = sourceItem as MapItem;
            if (child == null)
                continue;
            var location = child.Location;

            var existingAnnotation = _annotations.FirstOrDefault(a => a.Target == child);
            if (existingAnnotation == null)
            {
                var content = ItemTemplate.Build(child.DataContext ?? child);
                if (content == null)
                    continue;
                content.DataContext = child.DataContext ?? child;

                var annotation = content;
                var connector = new Line { Stroke = Stroke, StrokeThickness = StrokeThickness };

                var anchorPos = ConvertToScreen(location);
                var initialDirection = GetInitialDirection(_annotations.Count);
                var initialOffset = initialDirection * 50;

                var item = new MapAnnotation(child, annotation)
                {
                    Connector = connector,
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
            return;
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var position = e.GetPosition(this);
            var hitAnnotation = _annotations.FirstOrDefault(a =>
                a.Annotation.Bounds.Contains(position) || a.Target.Bounds.Contains(position)
            );

            if (hitAnnotation != null)
            {
                // Toggle the IsSelected state of the Target
                //hitAnnotation.Target.IsSelected = true;
                Source.Selection.Clear();
                Source.Selection.Select(Source.IndexFromContainer(hitAnnotation.Target));
                hitAnnotation.Target.Focus();
                UpdateVisuals(); // Redraw to display the change
                e.Handled = true;
            }
        }
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
            return new Point(0, 0);

        var tileSize = Source.Provider.TileSize;
        var projection = Source.Provider.Projection;
        var centerPixel = projection.Wgs84ToPixels(Source.CenterMap, Source.Zoom, tileSize);
        var offset = new Point(
            Source.Bounds.Width * 0.5 - centerPixel.X,
            Source.Bounds.Height * 0.5 - centerPixel.Y
        );
        return projection.Wgs84ToPixels(geoPoint, Source.Zoom, tileSize) + offset;
    }

    private void UpdateVisuals()
    {
        foreach (var item in _annotations)
        {
            // Center the TextBlock relative to the ScreenPosition
            var textWidth = item.Annotation.Bounds.Width;
            var textHeight = item.Annotation.Bounds.Height;
            SetLeft(item.Annotation, item.ScreenPosition.X - textWidth / 2);
            SetTop(item.Annotation, item.ScreenPosition.Y - textHeight / 2);

            var anchorPos = ConvertToScreen(item.AnchorPoint);
            item.Connector.StartPoint = anchorPos;
            item.Connector.EndPoint = item.ScreenPosition;
        }
    }

    public void ArrangeAnnotations(Size finalSize)
    {
        if (Source == null || _annotations.Count == 0)
            return;
        Debug.WriteLine("ArrangeAnnotations");
        const int baseMaxIterations = 100;
        const double repulsionStrength = 1000.0;
        const double attractionStrength = 0.1;
        const double damping = 0.9;
        const double minDistance = 50.0;
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
                var preferredPos = anchorPos + preferredDirection * minDistance;
                velocity += (preferredPos - currentPos) * attractionStrength;

                // Repulsion from other annotations with optimization
                foreach (var other in _annotations)
                {
                    if (other == item)
                        continue;

                    var delta = currentPos - other.ScreenPosition;
                    var distanceSquared = delta.LengthSquared(); // Faster than Length

                    if (distanceSquared < minDistance * minDistance) // Check squared distance
                    {
                        var distance = Math.Sqrt(distanceSquared);
                        var repulsion =
                            delta.Normalize() * (repulsionStrength / Math.Max(distance, 1.0));
                        velocity += repulsion;
                        stabilized = false;
                    }
                }

                // Limit velocity to avoid excessive jumps
                var velocityMagnitude = velocity.Length();
                if (velocityMagnitude > maxVelocity)
                {
                    velocity = velocity.Normalize() * maxVelocity;
                }

                // Update position
                var newPos = currentPos + velocity * damping;
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
                break;
        }

        UpdateVisuals();
    }
}

public class MapAnnotation : AsyncDisposableOnce
{
    private readonly IDisposable _dispose;

    public MapAnnotation(MapItem target, Control annotation)
    {
        Target = target;
        Annotation = annotation;
        _dispose = target
            .ObservePropertyChanged(x => x.IsSelected)
            .Subscribe(x => Annotation.Classes.Set("active", x));
    }

    public MapItem Target { get; }
    public Control Annotation { get; set; }
    public Line Connector { get; set; }
    public GeoPoint AnchorPoint { get; set; }
    public Point ScreenPosition { get; set; }

    protected override void Dispose(bool disposing)
    {
        _dispose.Dispose();
        base.Dispose(disposing);
    }
}
