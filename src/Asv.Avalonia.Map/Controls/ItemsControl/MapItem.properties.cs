using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;
using Asv.Common;

namespace Asv.Avalonia.Map;

public partial class MapItem
{
    public static readonly StyledProperty<VerticalOffset> CenterYProperty =
        AvaloniaProperty.Register<MapItem, VerticalOffset>(nameof(CenterY));

    public VerticalOffset CenterY
    {
        get => GetValue(CenterYProperty);
        set => SetValue(CenterYProperty, value);
    }

    public static readonly StyledProperty<HorizontalOffset> CenterXProperty =
        AvaloniaProperty.Register<MapItem, HorizontalOffset>(nameof(CenterX));

    public HorizontalOffset CenterX
    {
        get => GetValue(CenterXProperty);
        set => SetValue(CenterXProperty, value);
    }

    public static readonly StyledProperty<GeoPoint> LocationProperty =
        AvaloniaProperty.Register<MapItem, GeoPoint>(nameof(Location));

    public GeoPoint Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

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

    public static readonly StyledProperty<double> RotationProperty =
        AvaloniaProperty.Register<MapItem, double>(nameof(Rotation));

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<MapItem, bool>(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<MapItem, bool>(nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

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

    public static readonly StyledProperty<bool> IsPolygonClosedProperty =
        AvaloniaProperty.Register<MapItem, bool>(nameof(IsPolygonClosed));

    public bool IsPolygonClosed
    {
        get => GetValue(IsPolygonClosedProperty);
        set => SetValue(IsPolygonClosedProperty, value);
    }

    public static readonly StyledProperty<IPen?> PenProperty =
        AvaloniaProperty.Register<MapItem, IPen?>(nameof(Pen));

    public IPen? Pen
    {
        get => GetValue(PenProperty);
        set => SetValue(PenProperty, value);
    }

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<MapItem, IBrush?>(nameof(Fill));

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
}