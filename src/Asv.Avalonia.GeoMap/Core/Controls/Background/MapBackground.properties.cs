using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Asv.Avalonia.GeoMap;

public partial class MapBackground
{
    #region CenterMap

    private GeoPoint _centerMap;

    public static readonly DirectProperty<MapBackground, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, GeoPoint>(
            nameof(CenterMap),
            o => o.CenterMap,
            (o, v) => o.CenterMap = v
        );

    public GeoPoint CenterMap
    {
        get => _centerMap;
        set => SetAndRaise(CenterMapProperty, ref _centerMap, value);
    }

    #endregion

    #region Provider

    private ITileProvider _provider = EmptyTileProvider.Instance;

    public static readonly DirectProperty<MapBackground, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v,
            unsetValue: EmptyTileProvider.Instance
        );

    public ITileProvider Provider
    {
        get => _provider;
        set => SetAndRaise(ProviderProperty, ref _provider, value);
    }

    #endregion

    #region Zoom

    private ushort _zoom;

    public static readonly DirectProperty<MapBackground, ushort> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, ushort>(
            nameof(Zoom),
            o => o.Zoom,
            (o, v) => o.Zoom = v
        );

    public ushort Zoom
    {
        get => _zoom;
        set => SetAndRaise(ZoomProperty, ref _zoom, value);
    }

    #endregion

    #region IsDebug

    private bool _isDebug;

    public static readonly DirectProperty<MapBackground, bool> IsDebugEnabledProperty =
        AvaloniaProperty.RegisterDirect<MapBackground, bool>(
            nameof(IsDebug),
            o => o.IsDebug,
            (o, v) => o.IsDebug = v
        );

    public bool IsDebug
    {
        get => _isDebug;
        set => SetAndRaise(IsDebugEnabledProperty, ref _isDebug, value);
    }

    #endregion

    #region Background

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<MapBackground>();

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    #endregion
}
