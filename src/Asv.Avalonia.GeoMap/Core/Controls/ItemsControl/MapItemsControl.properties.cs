using Asv.Common;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Asv.Avalonia.GeoMap;

public partial class MapItemsControl
{
    #region ItemTemplate Property

    public static readonly StyledProperty<IDataTemplate?> AnnotationTemplateProperty =
        AvaloniaProperty.Register<MapItemsControl, IDataTemplate?>(nameof(AnnotationTemplate));

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? AnnotationTemplate
    {
        get => GetValue(AnnotationTemplateProperty);
        set => SetValue(AnnotationTemplateProperty, value);
    }

    #endregion

    #region Selection rect

    public static readonly DirectProperty<MapItemsControl, double> SelectionLeftProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionLeft),
            o => o.SelectionLeft,
            (o, v) => o.SelectionLeft = v
        );

    public double SelectionLeft
    {
        get;
        set => SetAndRaise(SelectionLeftProperty, ref field, value);
    }

    public static readonly DirectProperty<MapItemsControl, double> SelectionTopProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionTop),
            o => o.SelectionTop,
            (o, v) => o.SelectionTop = v
        );

    public double SelectionTop
    {
        get;
        set => SetAndRaise(SelectionTopProperty, ref field, value);
    }

    public static readonly DirectProperty<MapItemsControl, double> SelectionWidthProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionWidth),
            o => o.SelectionWidth,
            (o, v) => o.SelectionWidth = v
        );

    public double SelectionWidth
    {
        get;
        set => SetAndRaise(SelectionWidthProperty, ref field, value);
    }

    public static readonly DirectProperty<MapItemsControl, double> SelectionHeightProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionHeight),
            o => o.SelectionHeight,
            (o, v) => o.SelectionHeight = v
        );

    public double SelectionHeight
    {
        get;
        set => SetAndRaise(SelectionHeightProperty, ref field, value);
    }

    #endregion

    #region Drag state

    public static readonly DirectProperty<MapItemsControl, DragState> DragStateProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, DragState>(
            nameof(DragState),
            o => o.DragState,
            (o, v) => o.DragState = v
        );

    public DragState DragState
    {
        get;
        set => SetAndRaise(DragStateProperty, ref field, value);
    }

    #endregion

    #region Cursor position

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CursorPositionProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
            nameof(CursorPosition),
            o => o.CursorPosition,
            (o, v) => o.CursorPosition = v
        );

    public GeoPoint CursorPosition
    {
        get;
        set => SetAndRaise(CursorPositionProperty, ref field, value);
    }

    #endregion

    #region CenterMap

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
            nameof(CenterMap),
            o => o.CenterMap,
            (o, v) => o.CenterMap = v
        );

    public GeoPoint CenterMap
    {
        get;
        set => SetAndRaise(CenterMapProperty, ref field, value);
    }

    #endregion

    #region Provider

    public static readonly DirectProperty<MapItemsControl, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v
        );

    public ITileProvider Provider
    {
        get;
        set => SetAndRaise(ProviderProperty, ref field, value);
    } = EmptyTileProvider.Instance;

    #endregion

    #region Zoom

    private int _zoom = 8;

    public static readonly DirectProperty<MapItemsControl, int> ZoomProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, int>(
            nameof(Zoom),
            o => o.Zoom,
            (o, v) => o.Zoom = v
        );

    public int Zoom
    {
        get => _zoom;
        set => SetAndRaise(ZoomProperty, ref _zoom, value);
    }

    #endregion
}
