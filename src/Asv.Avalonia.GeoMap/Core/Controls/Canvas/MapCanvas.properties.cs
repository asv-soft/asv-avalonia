using Asv.Common;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.GeoMap;

public partial class MapCanvas
{
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
