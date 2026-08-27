using Asv.Common;
using Avalonia;

namespace Asv.Avalonia.GeoMap;

public sealed partial class MapScaleControl
{
    private double _actualBarWidth = DefaultMaxBarWidth;
    private double _scaleDistanceMeters;
    private string _middleText = string.Empty;
    private string _maximumText = string.Empty;
    private bool _isScaleValid;

    #region CenterMap

    public static readonly StyledProperty<GeoPoint> CenterMapProperty = AvaloniaProperty.Register<
        MapScaleControl,
        GeoPoint
    >(nameof(CenterMap), GeoPoint.Zero);

    public GeoPoint CenterMap
    {
        get => GetValue(CenterMapProperty);
        set => SetValue(CenterMapProperty, value);
    }

    #endregion

    #region Zoom

    public static readonly StyledProperty<int> ZoomProperty = AvaloniaProperty.Register<
        MapScaleControl,
        int
    >(nameof(Zoom), 10);

    public int Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    #endregion

    #region Provider

    public static readonly StyledProperty<ITileProvider?> ProviderProperty =
        AvaloniaProperty.Register<MapScaleControl, ITileProvider?>(
            nameof(Provider),
            EmptyTileProvider.Instance
        );

    public ITileProvider? Provider
    {
        get => GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    #endregion

    #region UnitService

    public static readonly StyledProperty<IUnitService?> UnitServiceProperty =
        AvaloniaProperty.Register<MapScaleControl, IUnitService?>(nameof(UnitService));

    public IUnitService? UnitService
    {
        get => GetValue(UnitServiceProperty);
        set => SetValue(UnitServiceProperty, value);
    }

    #endregion

    #region MaxBarWidth

    public static readonly StyledProperty<double> MaxBarWidthProperty = AvaloniaProperty.Register<
        MapScaleControl,
        double
    >(
        nameof(MaxBarWidth),
        DefaultMaxBarWidth,
        coerce: static (_, value) =>
            double.IsFinite(value) && value >= 1 ? value : DefaultMaxBarWidth
    );

    public double MaxBarWidth
    {
        get => GetValue(MaxBarWidthProperty);
        set => SetValue(MaxBarWidthProperty, value);
    }

    #endregion

    #region ActualBarWidth

    public static readonly DirectProperty<MapScaleControl, double> ActualBarWidthProperty =
        AvaloniaProperty.RegisterDirect<MapScaleControl, double>(
            nameof(ActualBarWidth),
            o => o.ActualBarWidth
        );

    public double ActualBarWidth
    {
        get => _actualBarWidth;
        private set => SetAndRaise(ActualBarWidthProperty, ref _actualBarWidth, value);
    }

    #endregion

    #region ScaleDistanceMeters

    public static readonly DirectProperty<MapScaleControl, double> ScaleDistanceMetersProperty =
        AvaloniaProperty.RegisterDirect<MapScaleControl, double>(
            nameof(ScaleDistanceMeters),
            o => o.ScaleDistanceMeters
        );

    /// <summary>
    /// Gets the distance represented by the scale bar, in meters.
    /// </summary>
    public double ScaleDistanceMeters
    {
        get => _scaleDistanceMeters;
        private set =>
            SetAndRaise(ScaleDistanceMetersProperty, ref _scaleDistanceMeters, value);
    }

    #endregion

    #region MiddleText

    public static readonly DirectProperty<MapScaleControl, string> MiddleTextProperty =
        AvaloniaProperty.RegisterDirect<MapScaleControl, string>(
            nameof(MiddleText),
            o => o.MiddleText
        );

    public string MiddleText
    {
        get => _middleText;
        private set => SetAndRaise(MiddleTextProperty, ref _middleText, value);
    }

    #endregion

    #region MaximumText

    public static readonly DirectProperty<MapScaleControl, string> MaximumTextProperty =
        AvaloniaProperty.RegisterDirect<MapScaleControl, string>(
            nameof(MaximumText),
            o => o.MaximumText
        );

    public string MaximumText
    {
        get => _maximumText;
        private set => SetAndRaise(MaximumTextProperty, ref _maximumText, value);
    }

    #endregion

    #region IsScaleValid

    public static readonly DirectProperty<MapScaleControl, bool> IsScaleValidProperty =
        AvaloniaProperty.RegisterDirect<MapScaleControl, bool>(
            nameof(IsScaleValid),
            o => o.IsScaleValid
        );

    public bool IsScaleValid
    {
        get => _isScaleValid;
        private set => SetAndRaise(IsScaleValidProperty, ref _isScaleValid, value);
    }

    #endregion
}
