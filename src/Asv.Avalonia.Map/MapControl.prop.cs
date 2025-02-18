using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.Map;

public partial class MapControl
{
    #region Cursor position

    private GeoPoint _cursorPosition;

    public static readonly DirectProperty<MapControl, GeoPoint> CursorPositionProperty =
        AvaloniaProperty.RegisterDirect<MapControl, GeoPoint>(
            nameof(CursorPosition),
            o => o.CursorPosition,
            (o, v) => o.CursorPosition = v
        );

    public GeoPoint CursorPosition
    {
        get => _cursorPosition;
        set => SetAndRaise(CursorPositionProperty, ref _cursorPosition, value);
    }

    #endregion

    #region CenterMap

    private GeoPoint _centerMap;

    public static readonly DirectProperty<MapControl, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapControl, GeoPoint>(
            nameof(CenterMap),
            o => o.CenterMap,
            (o, v) => o.CenterMap = v
        );

    public GeoPoint CenterMap
    {
        get => _centerMap;
        set
        {
            if (SetAndRaise(CenterMapProperty, ref _centerMap, value))
            {
                RequestRenderLoop();
            }
        }
    }

    #endregion

    #region Provider

    private ITileProvider _provider;
    public static readonly DirectProperty<MapControl, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapControl, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v
        );

    public ITileProvider Provider
    {
        get => _provider;
        set
        {
            if (SetAndRaise(ProviderProperty, ref _provider, value))
            {
                ProviderChanged(_provider);
            }
        }
    }

    private void ProviderChanged(ITileProvider provider)
    {
        _cache.Dispose();
        _cache = new CacheTileLoader(MapCore.LoggerFactory, provider);
        RequestRenderLoop();
    }

    #endregion

    #region Zoom

    private int _zoom;

    public static readonly DirectProperty<MapControl, int> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapControl, int>(
            nameof(Zoom),
            o => o.Zoom,
            (o, v) => o.Zoom = v
        );

    public int Zoom
    {
        get => _zoom;
        set
        {
            if (SetAndRaise(ZoomProperty, ref _zoom, value))
            {
                RequestRenderLoop();
            }
        }
    }

    #endregion

    #region IsDebug

    private bool _isDebug;

    public static readonly DirectProperty<MapControl, bool> IsDebugEnabledProperty =
        AvaloniaProperty.RegisterDirect<MapControl, bool>(
            nameof(IsDebug),
            o => o.IsDebug,
            (o, v) => o.IsDebug = v
        );

    public bool IsDebug
    {
        get => _isDebug;
        set
        {
            if (SetAndRaise(IsDebugEnabledProperty, ref _isDebug, value))
            {
                RequestRenderLoop();
            }
        }
    }

    #endregion

    #region Background

    public static readonly StyledProperty<IBrush> BackgroundProperty = AvaloniaProperty.Register<
        MapControl,
        IBrush
    >(nameof(Background), Brushes.CornflowerBlue);

    /// <summary>
    /// Gets or sets the brush used to draw the map background.
    /// </summary>
    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    #endregion
}
