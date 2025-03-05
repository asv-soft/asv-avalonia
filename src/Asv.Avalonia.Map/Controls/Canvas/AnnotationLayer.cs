using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
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
                layer.UpdateAnnotationsFromChildren();
            }
        });
    }

    private readonly List<MapAnnotation> _annotations = new();
    private Subject<Unit> _renderRequestSubject = new();

    public AnnotationLayer()
    {
        DisposableBuilder disposeBuilder = new();
        _renderRequestSubject.AddTo(ref disposeBuilder);
        _renderRequestSubject
            .ThrottleLastFrame(1)
            .Subscribe(_ => UpdateAnnotationsFromChildren())
            .AddTo(ref disposeBuilder);
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

    protected override Size ArrangeOverride(Size finalSize)
    {
        _renderRequestSubject.OnNext(Unit.Default);
        return base.ArrangeOverride(finalSize);
    }

    public void UpdateAnnotationsFromChildren()
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
                // Переключаем состояние IsSelected у Target
                //hitAnnotation.Target.IsSelected = true;
                Source.Selection.Clear();
                Source.Selection.Select(Source.IndexFromContainer(hitAnnotation.Target));
                UpdateVisuals(); // Перерисовываем для отображения изменения
                e.Handled = true;
            }
        }
    }

    private Point GetInitialDirection(int index)
    {
        // Распределяем аннотации по окружности
        double angle = 2 * Math.PI * index / 8; // 8 — примерное количество направлений
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
            // Центрируем TextBlock относительно ScreenPosition
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

        const int baseMaxIterations = 100;
        const double repulsionStrength = 1000.0;
        const double attractionStrength = 0.1;
        const double damping = 0.9;
        const double minDistance = 50.0;
        const double maxVelocity = 10.0; // Ограничение скорости для предотвращения скачков
        const double stabilizationThreshold = 0.1; // Порог стабилизации (в пикселях)

        // Адаптивное количество итераций: уменьшаем при большом числе аннотаций
        int maxIterations = Math.Min(baseMaxIterations, 1000 / Math.Max(1, _annotations.Count));

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            bool stabilized = true;

            foreach (var item in _annotations)
            {
                var currentPos = item.ScreenPosition;
                var anchorPos = ConvertToScreen(item.AnchorPoint);
                var velocity = new Point(0, 0);

                // Притяжение к предпочтительной радиальной позиции
                var preferredDirection = (currentPos - anchorPos).Normalize();
                var preferredPos = anchorPos + preferredDirection * minDistance;
                velocity += (preferredPos - currentPos) * attractionStrength;

                // Отталкивание от других аннотаций с оптимизацией
                foreach (var other in _annotations)
                {
                    if (other == item)
                        continue;

                    var delta = currentPos - other.ScreenPosition;
                    var distanceSquared = delta.LengthSquared(); // Быстрее, чем Length

                    if (distanceSquared < minDistance * minDistance) // Проверяем квадрат расстояния
                    {
                        var distance = Math.Sqrt(distanceSquared);
                        var repulsion =
                            delta.Normalize() * (repulsionStrength / Math.Max(distance, 1.0));
                        velocity += repulsion;
                        stabilized = false;
                    }
                }

                // Ограничиваем скорость, чтобы избежать чрезмерных скачков
                var velocityMagnitude = velocity.Length();
                if (velocityMagnitude > maxVelocity)
                {
                    velocity = velocity.Normalize() * maxVelocity;
                }

                // Обновляем позицию
                var newPos = currentPos + velocity * damping;
                newPos = new Point(
                    Math.Clamp(newPos.X, 0, finalSize.Width - item.Annotation.Bounds.Width),
                    Math.Clamp(newPos.Y, 0, finalSize.Height - item.Annotation.Bounds.Height)
                );

                // Проверяем, достаточно ли малая разница для стабилизации
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
