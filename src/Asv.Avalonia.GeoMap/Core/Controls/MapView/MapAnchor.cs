using Asv.Common;
using Asv.Modeling;
using Avalonia.Media;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapAnchor : ViewModel, IMapAnchor
{
    private readonly MapAnchorCore _core;

    public MapAnchor(string typeId, NavArgs args = default, GeoPoint? location = null)
        : base(typeId, args)
    {
        _core = new MapAnchorCore(this, Disposable, Id.ToString(), location);
    }

    public ObservableList<IMenuItem> Menu => _core.Menu;

    public MenuTree MenuView => _core.MenuView;

    public string? Header
    {
        get => _core.Header;
        set => SetField(ref _core.Header, value);
    }

    public double Azimuth
    {
        get => _core.Azimuth;
        set => SetField(ref _core.Azimuth, value);
    }

    public GeoPoint Location
    {
        get => _core.Location;
        set => SetField(ref _core.Location, value);
    }

    public double IconSize
    {
        get => _core.IconSize;
        set => SetField(ref _core.IconSize, value);
    }

    public MaterialIconKind? Icon
    {
        get => _core.Icon;
        set => SetField(ref _core.Icon, value);
    }

    public AsvColorKind IconColor
    {
        get => _core.IconColor;
        set => SetField(ref _core.IconColor, value);
    }

    public HorizontalOffset CenterX
    {
        get => _core.CenterX;
        set => SetField(ref _core.CenterX, value);
    }

    public VerticalOffset CenterY
    {
        get => _core.CenterY;
        set => SetField(ref _core.CenterY, value);
    }

    public bool IsReadOnly
    {
        get => _core.IsReadOnly;
        set => SetField(ref _core.IsReadOnly, value);
    }

    public bool CanDragWithoutModifier
    {
        get => _core.CanDragWithoutModifier;
        set => SetField(ref _core.CanDragWithoutModifier, value);
    }

    public bool UseMapRotation
    {
        get => _core.UseMapRotation;
        set => SetField(ref _core.UseMapRotation, value);
    }

    public bool IsSelected
    {
        get => _core.IsSelected;
        set => SetField(ref _core.IsSelected, value);
    }

    public bool IsVisible
    {
        get => _core.IsVisible;
        set => SetField(ref _core.IsVisible, value);
    }

    public IPen? PolygonPen
    {
        get => _core.PolygonPen;
        set => SetField(ref _core.PolygonPen, value);
    }

    public IBrush? PolygonFill
    {
        get => _core.PolygonFill;
        set => SetField(ref _core.PolygonFill, value);
    }

    public bool IsPolygonClosed
    {
        get => _core.IsPolygonClosed;
        set => SetField(ref _core.IsPolygonClosed, value);
    }

    public bool IsAnnotationVisible
    {
        get => _core.IsAnnotationVisible;
        set => SetField(ref _core.IsAnnotationVisible, value);
    }

    public ObservableList<GeoPoint> Polygon => _core.Polygon;

    public NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView => _core.PolygonView;

    public override IEnumerable<IViewModel> GetChildren()
    {
        return _core.GetChildren();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _core.Dispose();
        }

        base.Dispose(disposing);
    }
}

public class MapAnchor<TContext> : ViewModel<TContext>, IMapAnchor
    where TContext : class, IMapAnchor
{
    private readonly MapAnchorCore _core;

    public MapAnchor(string id, IExtensionService ext, GeoPoint? location = null)
        : this(id, default, ext, location) { }

    public MapAnchor(string id, NavArgs args, IExtensionService ext, GeoPoint? location = null)
        : base(id, args, ext)
    {
        _core = new MapAnchorCore(this, Disposable, Id.ToString(), location);
    }

    public ObservableList<IMenuItem> Menu => _core.Menu;

    public MenuTree MenuView => _core.MenuView;

    public string? Header
    {
        get => _core.Header;
        set => SetField(ref _core.Header, value);
    }

    public double Azimuth
    {
        get => _core.Azimuth;
        set => SetField(ref _core.Azimuth, value);
    }

    public GeoPoint Location
    {
        get => _core.Location;
        set => SetField(ref _core.Location, value);
    }

    public double IconSize
    {
        get => _core.IconSize;
        set => SetField(ref _core.IconSize, value);
    }

    public MaterialIconKind? Icon
    {
        get => _core.Icon;
        set => SetField(ref _core.Icon, value);
    }

    public AsvColorKind IconColor
    {
        get => _core.IconColor;
        set => SetField(ref _core.IconColor, value);
    }

    public HorizontalOffset CenterX
    {
        get => _core.CenterX;
        set => SetField(ref _core.CenterX, value);
    }

    public VerticalOffset CenterY
    {
        get => _core.CenterY;
        set => SetField(ref _core.CenterY, value);
    }

    public bool IsReadOnly
    {
        get => _core.IsReadOnly;
        set => SetField(ref _core.IsReadOnly, value);
    }

    public bool CanDragWithoutModifier
    {
        get => _core.CanDragWithoutModifier;
        set => SetField(ref _core.CanDragWithoutModifier, value);
    }

    public bool UseMapRotation
    {
        get => _core.UseMapRotation;
        set => SetField(ref _core.UseMapRotation, value);
    }

    public bool IsSelected
    {
        get => _core.IsSelected;
        set => SetField(ref _core.IsSelected, value);
    }

    public bool IsVisible
    {
        get => _core.IsVisible;
        set => SetField(ref _core.IsVisible, value);
    }

    public IPen? PolygonPen
    {
        get => _core.PolygonPen;
        set => SetField(ref _core.PolygonPen, value);
    }

    public IBrush? PolygonFill
    {
        get => _core.PolygonFill;
        set => SetField(ref _core.PolygonFill, value);
    }

    public bool IsPolygonClosed
    {
        get => _core.IsPolygonClosed;
        set => SetField(ref _core.IsPolygonClosed, value);
    }

    public bool IsAnnotationVisible
    {
        get => _core.IsAnnotationVisible;
        set => SetField(ref _core.IsAnnotationVisible, value);
    }

    public ObservableList<GeoPoint> Polygon => _core.Polygon;

    public NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView => _core.PolygonView;

    public override IEnumerable<IViewModel> GetChildren()
    {
        return _core.GetChildren();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _core.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}

internal sealed class MapAnchorCore : ISupportChildren<IViewModel>
{
    private string? _header;
    private double _azimuth;
    private GeoPoint _location;
    private double _iconSize = 40;
    private MaterialIconKind? _icon = MaterialIconKind.MapMarkerOutline;
    private AsvColorKind _iconColor;
    private HorizontalOffset _centerX;
    private VerticalOffset _centerY;
    private bool _isReadOnly;
    private bool _canDragWithoutModifier;
    private bool _useMapRotation;
    private bool _isSelected;
    private bool _isVisible = true;
    private IPen? _polygonPen = MapItem.DefaultPen;
    private IBrush? _polygonFill;
    private bool _isPolygonClosed;
    private bool _isAnnotationVisible;

    public MapAnchorCore(
        IMapAnchor owner,
        CompositeDisposable disposable,
        string defaultHeader,
        GeoPoint? location
    )
    {
        Header = defaultHeader;
        Location = location ?? GeoPoint.Zero;

        PolygonView = Polygon.ToNotifyCollectionChangedSlim().DisposeItWith(disposable);

        Menu.SetParent(owner).DisposeItWith(disposable);
        Menu.DisposeRemovedItems().DisposeItWith(disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(disposable);
    }

    public ObservableList<IMenuItem> Menu { get; } = [];
    public MenuTree MenuView { get; }
    public ObservableList<GeoPoint> Polygon { get; } = [];
    public NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView { get; }

    public ref string? Header => ref _header;
    public ref double Azimuth => ref _azimuth;
    public ref GeoPoint Location => ref _location;
    public ref double IconSize => ref _iconSize;
    public ref MaterialIconKind? Icon => ref _icon;
    public ref AsvColorKind IconColor => ref _iconColor;
    public ref HorizontalOffset CenterX => ref _centerX;
    public ref VerticalOffset CenterY => ref _centerY;
    public ref bool IsReadOnly => ref _isReadOnly;
    public ref bool CanDragWithoutModifier => ref _canDragWithoutModifier;
    public ref bool UseMapRotation => ref _useMapRotation;
    public ref bool IsSelected => ref _isSelected;
    public ref bool IsVisible => ref _isVisible;
    public ref IPen? PolygonPen => ref _polygonPen;
    public ref IBrush? PolygonFill => ref _polygonFill;
    public ref bool IsPolygonClosed => ref _isPolygonClosed;
    public ref bool IsAnnotationVisible => ref _isAnnotationVisible;

    public void Dispose()
    {
        Menu.RemoveAll();
    }

    public IEnumerable<IViewModel> GetChildren()
    {
        return Menu;
    }
}
