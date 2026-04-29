using Asv.Common;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Asv.Avalonia.GeoMap;

public partial class MapItemsControl
{
    #region Rotation

    public static readonly StyledProperty<double> RotationProperty = AvaloniaProperty.Register<
        MapItemsControl,
        double
    >(nameof(Rotation), defaultBindingMode: BindingMode.TwoWay);

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    #endregion

    #region Interaction

    public static readonly StyledProperty<bool> EnableWheelZoomProperty = AvaloniaProperty.Register<
        MapItemsControl,
        bool
    >(nameof(EnableWheelZoom), true);

    public bool EnableWheelZoom
    {
        get => GetValue(EnableWheelZoomProperty);
        set => SetValue(EnableWheelZoomProperty, value);
    }

    public static readonly StyledProperty<bool> EnableTouchpadGesturesProperty =
        AvaloniaProperty.Register<MapItemsControl, bool>(nameof(EnableTouchpadGestures), true);

    public bool EnableTouchpadGestures
    {
        get => GetValue(EnableTouchpadGesturesProperty);
        set => SetValue(EnableTouchpadGesturesProperty, value);
    }

    public static readonly StyledProperty<double> TouchpadZoomSensitivityProperty =
        AvaloniaProperty.Register<MapItemsControl, double>(nameof(TouchpadZoomSensitivity), 0.75);

    public double TouchpadZoomSensitivity
    {
        get => GetValue(TouchpadZoomSensitivityProperty);
        set => SetValue(TouchpadZoomSensitivityProperty, Math.Abs(value));
    }

    public static readonly StyledProperty<double> TouchpadMagnifyStepThresholdProperty =
        AvaloniaProperty.Register<MapItemsControl, double>(
            nameof(TouchpadMagnifyStepThreshold),
            0.55
        );

    public double TouchpadMagnifyStepThreshold
    {
        get => GetValue(TouchpadMagnifyStepThresholdProperty);
        set => SetValue(TouchpadMagnifyStepThresholdProperty, value);
    }

    public static readonly StyledProperty<double> WheelDiscreteStepThresholdProperty =
        AvaloniaProperty.Register<MapItemsControl, double>(
            nameof(WheelDiscreteStepThreshold),
            0.95
        );

    public double WheelDiscreteStepThreshold
    {
        get => GetValue(WheelDiscreteStepThresholdProperty);
        set => SetValue(WheelDiscreteStepThresholdProperty, value);
    }

    #endregion

    #region ItemTemplate Property

    public static readonly StyledProperty<IDataTemplate?> AnnotationTemplateProperty =
        AvaloniaProperty.Register<MapItemsControl, IDataTemplate?>(nameof(AnnotationTemplate));

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? AnnotationTemplate
    {
        get => GetValue(AnnotationTemplateProperty);
        set => SetValue(AnnotationTemplateProperty, value);
    }

    public static readonly StyledProperty<double> AnnotationRadiusProperty =
        AvaloniaProperty.Register<AnnotationLayer, double>(nameof(AnnotationRadius), 50.0);

    public double AnnotationRadius
    {
        get => GetValue(AnnotationRadiusProperty);
        set => SetValue(AnnotationRadiusProperty, value);
    }

    #endregion

    #region Selection rect

    public static readonly DirectProperty<MapItemsControl, double> SelectionLeftProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionLeft),
            o => o.SelectionLeft,
            (o, v) => o.SelectionLeft = v
        );

    public double SelectionLeft
    {
        get;
        set => SetAndRaise(SelectionLeftProperty, ref field, value);
    }

    public static readonly DirectProperty<MapItemsControl, double> SelectionTopProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionTop),
            o => o.SelectionTop,
            (o, v) => o.SelectionTop = v
        );

    public double SelectionTop
    {
        get;
        set => SetAndRaise(SelectionTopProperty, ref field, value);
    }

    public static readonly DirectProperty<MapItemsControl, double> SelectionWidthProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionWidth),
            o => o.SelectionWidth,
            (o, v) => o.SelectionWidth = v
        );

    public double SelectionWidth
    {
        get;
        set => SetAndRaise(SelectionWidthProperty, ref field, value);
    }

    public static readonly StyledProperty<IBrush?> SelectionStrokeProperty =
        AvaloniaProperty.Register<MapItemsControl, IBrush?>(
            nameof(SelectionStroke),
            Brushes.DarkViolet
        );

    public IBrush? SelectionStroke
    {
        get => GetValue(SelectionStrokeProperty);
        set => SetValue(SelectionStrokeProperty, value);
    }

    public static readonly StyledProperty<double> SelectionStrokeThicknessProperty =
        AvaloniaProperty.Register<MapItemsControl, double>(nameof(SelectionStrokeThickness), 2);

    public double SelectionStrokeThickness
    {
        get => GetValue(SelectionStrokeThicknessProperty);
        set => SetValue(SelectionStrokeThicknessProperty, value);
    }

    public static readonly DirectProperty<MapItemsControl, double> SelectionHeightProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionHeight),
            o => o.SelectionHeight,
            (o, v) => o.SelectionHeight = v
        );

    public double SelectionHeight
    {
        get;
        set => SetAndRaise(SelectionHeightProperty, ref field, value);
    }

    #endregion

    #region Drag state

    public static readonly DirectProperty<MapItemsControl, DragState> DragStateProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, DragState>(
            nameof(DragState),
            o => o.DragState,
            (o, v) => o.DragState = v
        );

    public DragState DragState
    {
        get;
        set => SetAndRaise(DragStateProperty, ref field, value);
    }

    #endregion

    #region Cursor position

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CursorPositionProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
            nameof(CursorPosition),
            o => o.CursorPosition,
            (o, v) => o.CursorPosition = v
        );

    public GeoPoint CursorPosition
    {
        get;
        set => SetAndRaise(CursorPositionProperty, ref field, value);
    }

    #endregion

    #region CenterMap

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
            nameof(CenterMap),
            o => o.CenterMap,
            (o, v) => o.CenterMap = v
        );

    public GeoPoint CenterMap
    {
        get;
        set => SetAndRaise(CenterMapProperty, ref field, value);
    }

    #endregion

    #region Provider

    public static readonly DirectProperty<MapItemsControl, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v
        );

    public ITileProvider Provider
    {
        get;
        set => SetAndRaise(ProviderProperty, ref field, value);
    } = EmptyTileProvider.Instance;

    #endregion

    #region Zoom

    private int _zoom = 8;

    public static readonly DirectProperty<MapItemsControl, int> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, int>(
            nameof(Zoom),
            o => o.Zoom,
            (o, v) => o.Zoom = v
        );

    public int Zoom
    {
        get => _zoom;
        set =>
            SetAndRaise(
                ZoomProperty,
                ref _zoom,
                Math.Clamp(
                    Math.Clamp(value, MinZoom, MaxZoom),
                    Provider?.Info.MinZoom ?? 0,
                    Provider?.Info.MaxZoom ?? MaxZoom
                )
            );
    }

    #endregion

    #region MinZoom

    public static readonly DirectProperty<MapItemsControl, int> MinZoomProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, int>(
            nameof(MinZoom),
            o => o.MinZoom,
            (o, v) => o.MinZoom = v
        );

    public int MinZoom
    {
        get;
        set =>
            SetAndRaise(
                MinZoomProperty,
                ref field,
                Math.Clamp(value, IZoomService.MinZoomLevel, MaxZoom)
            );
    } = IZoomService.MinZoomLevel;

    #endregion

    #region MaxZoom

    public static readonly DirectProperty<MapItemsControl, int> MaxZoomProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, int>(
            nameof(MaxZoom),
            o => o.MaxZoom,
            (o, v) => o.MaxZoom = v
        );

    public int MaxZoom
    {
        get;
        set =>
            SetAndRaise(
                MaxZoomProperty,
                ref field,
                Math.Clamp(value, MinZoom, IZoomService.MaxZoomLevel)
            );
    } = IZoomService.MaxZoomLevel;

    #endregion
}
