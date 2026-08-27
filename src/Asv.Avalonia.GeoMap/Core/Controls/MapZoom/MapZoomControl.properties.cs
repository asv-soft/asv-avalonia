using Avalonia;
using Avalonia.Data;

namespace Asv.Avalonia.GeoMap;

public sealed partial class MapZoomControl
{
    private bool _canZoomIn = true;
    private bool _canZoomOut = true;

    #region Zoom

    public static readonly StyledProperty<int> ZoomProperty = AvaloniaProperty.Register<
        MapZoomControl,
        int
    >(nameof(Zoom), 10, defaultBindingMode: BindingMode.TwoWay);

    public int Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    #endregion

    #region MinZoom

    public static readonly StyledProperty<int> MinZoomProperty = AvaloniaProperty.Register<
        MapZoomControl,
        int
    >(nameof(MinZoom), IZoomService.MinZoomLevel);

    public int MinZoom
    {
        get => GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    #endregion

    #region MaxZoom

    public static readonly StyledProperty<int> MaxZoomProperty = AvaloniaProperty.Register<
        MapZoomControl,
        int
    >(nameof(MaxZoom), IZoomService.MaxZoomLevel);

    public int MaxZoom
    {
        get => GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    #endregion

    #region CanZoomIn

    public static readonly DirectProperty<MapZoomControl, bool> CanZoomInProperty =
        AvaloniaProperty.RegisterDirect<MapZoomControl, bool>(nameof(CanZoomIn), o => o.CanZoomIn);

    public bool CanZoomIn
    {
        get => _canZoomIn;
        private set => SetAndRaise(CanZoomInProperty, ref _canZoomIn, value);
    }

    #endregion

    #region CanZoomOut

    public static readonly DirectProperty<MapZoomControl, bool> CanZoomOutProperty =
        AvaloniaProperty.RegisterDirect<MapZoomControl, bool>(
            nameof(CanZoomOut),
            o => o.CanZoomOut
        );

    public bool CanZoomOut
    {
        get => _canZoomOut;
        private set => SetAndRaise(CanZoomOutProperty, ref _canZoomOut, value);
    }

    #endregion
}
