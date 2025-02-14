using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia.Map;

public class MapPanel : Panel
{
    static MapPanel()
    {
        AffectsParentArrange<MapPanel>(LocationProperty);
        AffectsArrange<MapPanel>(ProviderProperty, ZoomProperty, CenterMapProperty);
    }

    public MapPanel()
    {
        Provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
    }

    #region Location

    public static readonly AttachedProperty<GeoPoint?> LocationProperty =
        AvaloniaProperty.RegisterAttached<MapPanel, Control, GeoPoint?>("Location");

    public static GeoPoint? GetLocation(Control element) => element.GetValue(LocationProperty);

    public static void SetLocation(Control element, GeoPoint? value) =>
        element.SetValue(LocationProperty, value);

    #endregion

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
        if (point == null)
            return;
        if (Provider == null)
            return;

        var centerPixel = Provider.Projection.Wgs84ToPixels(CenterMap, Zoom, Provider.TileSize);
        var offset = new Point(Bounds.Width / 2 - centerPixel.X, Bounds.Height / 2 - centerPixel.Y);
        var pos = Provider.Projection.Wgs84ToPixels(point.Value, Zoom, Provider.TileSize);
        pos += offset;

        child.Arrange(new Rect(pos, child.DesiredSize));
    }

    #region CenterMap

    public static readonly StyledProperty<GeoPoint> CenterMapProperty = AvaloniaProperty.Register<
        MapPanel,
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
        AvaloniaProperty.Register<MapPanel, ITileProvider?>(
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
        MapPanel,
        int
    >(nameof(Zoom));

    public int Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    #endregion
}
