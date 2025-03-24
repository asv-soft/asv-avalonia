using Asv.Common;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Media;

namespace Asv.Avalonia.Map;

[PseudoClasses(":pressed", ":selected", ":pointerover")]
public class MapItem : ContentControl, ISelectable
{
    static MapItem()
    {
        SelectableMixin.Attach<MapItem>(IsSelectedProperty);
        PressedMixin.Attach<MapItem>();
        CenterYProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapItem));
        CenterYProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapItem));
        BoundsProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapItem));
    }

    private static void RecalculateRotation(MapItem? sender)
    {
        if (sender == null)
        {
            return;
        }

        sender.RotationCenterX = sender.CenterX.CalculateOffset(sender.Bounds.Width);
        sender.RotationCenterY = sender.CenterY.CalculateOffset(sender.Bounds.Height);
    }

    public MapItem() { }

    #region CenterY

    public static readonly StyledProperty<VerticalOffset> CenterYProperty =
        AvaloniaProperty.Register<MapItem, VerticalOffset>(nameof(CenterY));

    public VerticalOffset CenterY
    {
        get => GetValue(CenterYProperty);
        set => SetValue(CenterYProperty, value);
    }

    #endregion

    #region CenterX

    public static readonly StyledProperty<HorizontalOffset> CenterXProperty =
        AvaloniaProperty.Register<MapItem, HorizontalOffset>(nameof(CenterX));

    public HorizontalOffset CenterX
    {
        get => GetValue(CenterXProperty);
        set => SetValue(CenterXProperty, value);
    }

    #endregion

    #region Location

    public static readonly StyledProperty<GeoPoint> LocationProperty = AvaloniaProperty.Register<
        MapItem,
        GeoPoint
    >(nameof(Location));

    public GeoPoint Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    #endregion

    #region RotationCenterX

    private double _rotationCenterX;

    public static readonly DirectProperty<MapItem, double> RotationCenterXProperty =
        AvaloniaProperty.RegisterDirect<MapItem, double>(
            nameof(RotationCenterX),
            o => o.RotationCenterX,
            (o, v) => o.RotationCenterX = v
        );

    public double RotationCenterX
    {
        get => _rotationCenterX;
        set => SetAndRaise(RotationCenterXProperty, ref _rotationCenterX, value);
    }

    #endregion

    #region RotationCenterY

    private double _rotationCenterY;

    public static readonly DirectProperty<MapItem, double> RotationCenterYProperty =
        AvaloniaProperty.RegisterDirect<MapItem, double>(
            nameof(RotationCenterY),
            o => o.RotationCenterY,
            (o, v) => o.RotationCenterY = v
        );

    public double RotationCenterY
    {
        get => _rotationCenterY;
        set => SetAndRaise(RotationCenterYProperty, ref _rotationCenterY, value);
    }

    #endregion

    #region Rotation

    public static readonly StyledProperty<double> RotationProperty = AvaloniaProperty.Register<
        MapItem,
        double
    >(nameof(Rotation));

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    #endregion

    #region IsReadOnly

    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<
        MapItem,
        bool
    >(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion

    #region IsSelected

    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<
        MapItem,
        bool
    >(nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    #endregion

    #region Polygon

    private IList<GeoPoint>? _polygon;

    public static readonly DirectProperty<MapItem, IList<GeoPoint>?> PolygonProperty =
        AvaloniaProperty.RegisterDirect<MapItem, IList<GeoPoint>?>(
            nameof(Polygon),
            o => o.Polygon,
            (o, v) => o.Polygon = v
        );

    public IList<GeoPoint>? Polygon
    {
        get => _polygon;
        set => SetAndRaise(PolygonProperty, ref _polygon, value);
    }

    #endregion

    #region IsPolygonClosed

    public static readonly StyledProperty<bool> IsPolygonClosedProperty = AvaloniaProperty.Register<
        MapItem,
        bool
    >(nameof(IsPolygonClosed));

    public bool IsPolygonClosed
    {
        get => GetValue(IsPolygonClosedProperty);
        set => SetValue(IsPolygonClosedProperty, value);
    }

    #endregion

    #region Pen

    public static readonly StyledProperty<IPen?> PenProperty = AvaloniaProperty.Register<
        MapItem,
        IPen?
    >(nameof(Pen));

    public IPen? Pen
    {
        get => GetValue(PenProperty);
        set => SetValue(PenProperty, value);
    }

    #endregion

    #region Fill

    public static readonly StyledProperty<IBrush?> FillProperty = AvaloniaProperty.Register<
        MapItem,
        IBrush?
    >(nameof(Fill));

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    #endregion
}

public class GeoPointCollection : AvaloniaList<GeoPoint> { }
