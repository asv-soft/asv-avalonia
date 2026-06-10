using Asv.Common;
using Asv.Modeling;
using Avalonia.Media;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapAnchor : ViewModel, IMapAnchor
{
    public MapAnchor(string typeId, NavArgs args = default, GeoPoint? location = null)
        : base(typeId, args)
    {
        Icon = MaterialIconKind.MapMarkerOutline;
        Header = Id.ToString();
        Polygon = [];
        PolygonView = Polygon.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Location = location ?? GeoPoint.Zero;
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public double Azimuth
    {
        get;
        set => SetField(ref field, value);
    }

    public GeoPoint Location
    {
        get;
        set => SetField(ref field, value);
    }

    public double IconSize
    {
        get;
        set => SetField(ref field, value);
    } = 40;

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public HorizontalOffset CenterX
    {
        get;
        set => SetField(ref field, value);
    }

    public VerticalOffset CenterY
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsReadOnly
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanDragWithoutModifier
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseMapRotation
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public IPen? PolygonPen
    {
        get;
        set => SetField(ref field, value);
    } = MapItem.DefaultPen;

    public IBrush? PolygonFill
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsPolygonClosed
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsAnnotationVisible
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableList<GeoPoint> Polygon { get; }

    public NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}

public class MapAnchor<TContext> : ViewModel<TContext>, IMapAnchor
    where TContext : class, IMapAnchor
{
    public MapAnchor(string id, IExtensionService ext, GeoPoint? location = null)
        : this(id, default, ext, location) { }

    public MapAnchor(string id, NavArgs args, IExtensionService ext, GeoPoint? location = null)
        : base(id, args, ext)
    {
        Icon = MaterialIconKind.MapMarkerOutline;
        Header = Id.ToString();
        Polygon = [];
        PolygonView = Polygon.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Location = location ?? GeoPoint.Zero;
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public double Azimuth
    {
        get;
        set => SetField(ref field, value);
    }

    public GeoPoint Location
    {
        get;
        set => SetField(ref field, value);
    }

    public double IconSize
    {
        get;
        set => SetField(ref field, value);
    } = 40;

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public HorizontalOffset CenterX
    {
        get;
        set => SetField(ref field, value);
    }

    public VerticalOffset CenterY
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsReadOnly
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanDragWithoutModifier
    {
        get;
        set => SetField(ref field, value);
    }

    public bool UseMapRotation
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public IPen? PolygonPen
    {
        get;
        set => SetField(ref field, value);
    } = MapItem.DefaultPen;

    public IBrush? PolygonFill
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsPolygonClosed
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsAnnotationVisible
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableList<GeoPoint> Polygon { get; }

    public NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
