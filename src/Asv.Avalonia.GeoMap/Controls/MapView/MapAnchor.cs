using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapAnchor<TContext> : ExtendableViewModel<TContext>, IMapAnchor
    where TContext : class, IMapAnchor
{
    public MapAnchor(NavigationId id, ILoggerFactory loggerFactory)
        : this(id, loggerFactory, GeoPoint.Zero) { }

    public MapAnchor(NavigationId id, ILoggerFactory loggerFactory, GeoPoint? location = null)
        : base(id, loggerFactory)
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
    }

    public bool IsPolygonClosed
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

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }
}
