using System.Collections;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Asv.Avalonia.Map;

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

    #region Selection

    public new IList? SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }

    public new ISelectionModel Selection
    {
        get => base.Selection;
        set => base.Selection = value;
    }

    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }

    #endregion

    #region Selection rect

    private double _selectionLeft;

    public static readonly DirectProperty<MapItemsControl, double> SelectionLeftProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionLeft),
            o => o.SelectionLeft,
            (o, v) => o.SelectionLeft = v
        );

    public double SelectionLeft
    {
        get => _selectionLeft;
        set => SetAndRaise(SelectionLeftProperty, ref _selectionLeft, value);
    }

    private double _selectionTop;

    public static readonly DirectProperty<MapItemsControl, double> SelectionTopProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionTop),
            o => o.SelectionTop,
            (o, v) => o.SelectionTop = v
        );

    public double SelectionTop
    {
        get => _selectionTop;
        set => SetAndRaise(SelectionTopProperty, ref _selectionTop, value);
    }

    private double _selectionWidth;

    public static readonly DirectProperty<MapItemsControl, double> SelectionWidthProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionWidth),
            o => o.SelectionWidth,
            (o, v) => o.SelectionWidth = v
        );

    public double SelectionWidth
    {
        get => _selectionWidth;
        set => SetAndRaise(SelectionWidthProperty, ref _selectionWidth, value);
    }

    private double _selectionHeight;

    public static readonly DirectProperty<MapItemsControl, double> SelectionHeightProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, double>(
            nameof(SelectionHeight),
            o => o.SelectionHeight,
            (o, v) => o.SelectionHeight = v
        );

    public double SelectionHeight
    {
        get => _selectionHeight;
        set => SetAndRaise(SelectionHeightProperty, ref _selectionHeight, value);
    }

    #endregion

    #region Drag state

    private DragState _dragState;

    public static readonly DirectProperty<MapItemsControl, DragState> DragStateProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, DragState>(
            nameof(DragState),
            o => o.DragState,
            (o, v) => o.DragState = v
        );

    public DragState DragState
    {
        get => _dragState;
        set => SetAndRaise(DragStateProperty, ref _dragState, value);
    }

    #endregion

    #region Cursor position

    private GeoPoint _cursorPosition;

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CursorPositionProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
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

    public static readonly DirectProperty<MapItemsControl, GeoPoint> CenterMapProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, GeoPoint>(
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
    public static readonly DirectProperty<MapItemsControl, ITileProvider> ProviderProperty =
        AvaloniaProperty.RegisterDirect<MapItemsControl, ITileProvider>(
            nameof(Provider),
            o => o.Provider,
            (o, v) => o.Provider = v
        );

    public ITileProvider Provider
    {
        get => _provider;
        set => SetAndRaise(ProviderProperty, ref _provider, value);
    }

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