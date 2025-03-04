using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia.Map;

public class MapCanvas : Panel
{
    static MapCanvas()
    {
        AffectsParentArrange<MapCanvas>(LocationProperty);
        AffectsArrange<MapCanvas>(ProviderProperty, ZoomProperty, CenterMapProperty);
    }

    public MapCanvas()
    {
        Provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        foreach (Control child in Children)
        {
            child.Measure(availableSize);
        }

        return new Size();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (Control child in Children)
        {
            ArrangeChild(child, finalSize);
        }

        return finalSize;
    }

    protected virtual void ArrangeChild(Control child, Size finalSize)
    {
        var point = GetLocation(child);
        var offsetH = GetCenterX(child);
        var offsetV = GetCenterY(child);

        if (Provider == null || point == null)
            return;

        var tileSize = Provider.TileSize;
        var halfWidth = Bounds.Width * 0.5;
        var halfHeight = Bounds.Height * 0.5;
        var projection = Provider.Projection;

        var centerPixel = projection.Wgs84ToPixels(CenterMap, Zoom, tileSize);
        var offset = new Point(halfWidth - centerPixel.X, halfHeight - centerPixel.Y);
        var pos = projection.Wgs84ToPixels(point.Value, Zoom, tileSize);

        pos += offset;
        pos = new Point(
            pos.X - offsetH.CalculateOffset(child.Bounds.Width),
            pos.Y - offsetV.CalculateOffset(child.Bounds.Height)
        );

        child.Arrange(new Rect(pos, child.DesiredSize));
    }

    #region Rotation

    public static readonly AttachedProperty<double> RotationProperty =
        AvaloniaProperty.RegisterAttached<MapCanvas, Control, double>("Rotation");

    public static void SetRotation(Control obj, double value) =>
        obj.SetValue(RotationProperty, value);

    public static double GetRotation(Control obj) => obj.GetValue(RotationProperty);

    #endregion

    #region CenterY

    public static readonly AttachedProperty<VerticalOffset> CenterYProperty =
        AvaloniaProperty.RegisterAttached<MapCanvas, Control, VerticalOffset>("CenterY");

    public static void SetCenterY(Control obj, VerticalOffset value) =>
        obj.SetValue(CenterYProperty, value);

    public static VerticalOffset GetCenterY(Control obj) => obj.GetValue(CenterYProperty);

    #endregion

    #region CenterX

    public static readonly AttachedProperty<HorizontalOffset> CenterXProperty =
        AvaloniaProperty.RegisterAttached<MapCanvas, Control, HorizontalOffset>("CenterX");

    public static void SetCenterX(Control obj, HorizontalOffset value) =>
        obj.SetValue(CenterXProperty, value);

    public static HorizontalOffset GetCenterX(Control obj) => obj.GetValue(CenterXProperty);

    #endregion

    #region Location

    public static readonly AttachedProperty<GeoPoint?> LocationProperty =
        AvaloniaProperty.RegisterAttached<MapCanvas, Control, GeoPoint?>("Location");

    public static GeoPoint? GetLocation(Control element) => element.GetValue(LocationProperty);

    public static void SetLocation(Control element, GeoPoint? value) =>
        element.SetValue(LocationProperty, value);

    #endregion

    #region CenterMap

    public static readonly StyledProperty<GeoPoint> CenterMapProperty = AvaloniaProperty.Register<
        MapCanvas,
        GeoPoint
    >(nameof(CenterMap));

    public GeoPoint CenterMap
    {
        get => GetValue(CenterMapProperty);
        set => SetValue(CenterMapProperty, value);
    }

    #endregion

    #region Provider

    public static readonly StyledProperty<ITileProvider?> ProviderProperty =
        AvaloniaProperty.Register<MapCanvas, ITileProvider?>(
            nameof(Provider),
            coerce: (o, provider) => provider ?? EmptyTileProvider.Instance
        );

    public ITileProvider? Provider
    {
        get => GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    #endregion

    #region Zoom

    public static readonly StyledProperty<int> ZoomProperty = AvaloniaProperty.Register<
        MapCanvas,
        int
    >(nameof(Zoom));

    public int Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    #endregion
}
