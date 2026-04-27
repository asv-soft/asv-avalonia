using Asv.Common;
using Asv.Modeling;
using Avalonia.Media;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapAnchor<TContext> : ViewModel<TContext>, IMapAnchor
    where TContext : class, IMapAnchor
{
    public MapAnchor(string typeId)
        : this(typeId, NullExtensionService.Instance, GeoPoint.Zero) { }

    public MapAnchor(
        string id,
        IExtensionService ext,
        GeoPoint? location = null
    )
        : base(id, default, ext)
    {
        Title = id.ToString();
        Polygon = new ObservableList<GeoPoint>();
        PolygonView = Polygon.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        ReactiveLocation = new BindableReactiveProperty<GeoPoint>(
            location ?? GeoPoint.Zero
        ).DisposeItWith(Disposable);
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

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public IBrush Foreground
    {
        get;
        set => SetField(ref field, value);
    } = Brushes.NavajoWhite;

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

    public BindableReactiveProperty<GeoPoint> ReactiveLocation { get; }

    public string Title
    {
        get;
        set => SetField(ref field, value);
    }

    public override ValueTask<IViewModel> Navigate(NavId id)
    {
        return ValueTask.FromResult<IViewModel>(this);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
