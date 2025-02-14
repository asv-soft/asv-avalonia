using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;

namespace Asv.Avalonia.Map;

public partial class MapControl
{
    static MapControl()
    {
        AffectsRender<Panel>(BackgroundProperty);
    }

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
                var centerPixel = Provider.Projection.Wgs84ToPixels(value, Zoom, Provider.TileSize);
                _offset = new Point(
                    Bounds.Width / 2 - centerPixel.X,
                    Bounds.Height / 2 - centerPixel.Y
                );
                RequestRenderLoop();
            }
        }
    }

    #endregion

    #region Provider

    private ITileProvider _provider = EmptyTileProvider.Instance;
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

    #region DialogStroke

    public static readonly StyledProperty<IBrush?> DialogStrokeProperty = AvaloniaProperty.Register<
        MapControl,
        IBrush?
    >(nameof(DialogStroke), defaultValue: Brushes.Firebrick);

    public IBrush? DialogStroke
    {
        get => GetValue(DialogStrokeProperty);
        set => SetValue(DialogStrokeProperty, value);
    }

    #endregion

    #region DialogTargetLocation

    private GeoPoint? _dialogTargetLocation;

    public static readonly DirectProperty<MapControl, GeoPoint?> GeoPointProperty =
        AvaloniaProperty.RegisterDirect<MapControl, GeoPoint?>(
            nameof(DialogTargetLocation),
            o => o.DialogTargetLocation,
            (o, v) => o.DialogTargetLocation = v
        );

    public GeoPoint? DialogTargetLocation
    {
        get => _dialogTargetLocation;
        set => SetAndRaise(GeoPointProperty, ref _dialogTargetLocation, value);
    }

    #endregion

    #region DialogText

    private string? _dialogText;

    public static readonly DirectProperty<MapControl, string?> DialogTextProperty =
        AvaloniaProperty.RegisterDirect<MapControl, string?>(
            nameof(DialogText),
            o => o.DialogText,
            (o, v) => o.DialogText = v
        );

    public string? DialogText
    {
        get => _dialogText;
        set => SetAndRaise(DialogTextProperty, ref _dialogText, value);
    }

    #endregion

    #region IsDialogMode

    private bool _isDialogMode;

    public static readonly DirectProperty<MapControl, bool> IsDialogModeProperty =
        AvaloniaProperty.RegisterDirect<MapControl, bool>(
            nameof(IsDialogMode),
            o => o.IsDialogMode,
            (o, v) => o.IsDialogMode = v
        );

    public bool IsDialogMode
    {
        get => _isDialogMode;
        set => SetAndRaise(IsDialogModeProperty, ref _isDialogMode, value);
    }

    #endregion

    #region Location

    public static readonly AttachedProperty<GeoPoint?> LocationProperty =
        AvaloniaProperty.RegisterAttached<MapControl, Control, GeoPoint?>("Location");

    public static GeoPoint? GetLocation(Control element) => element.GetValue(LocationProperty);

    public static void SetLocation(Control element, GeoPoint? value) =>
        element.SetValue(LocationProperty, value);

    #endregion

    #region Background

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<Panel>();

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    #endregion
}
