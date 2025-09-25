using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMap : IRoutable
{
    ObservableList<IMapWidget> Widgets { get; }
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
}

public interface IMapWidget : IHeadlinedViewModel
{
    public WorkspaceDock Position { get; }
}

public class MapViewModel : RoutableViewModel, IMap
{
    public MapViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        var drone = new MapAnchor<IMapAnchor>(DesignTime.Id, DesignTime.LoggerFactory)
        {
            Icon = MaterialIconKind.Navigation,
        };
        Anchors.Add(drone);
        var azimuth = 0;
        TimeProvider.System.CreateTimer(
            _ =>
            {
                drone.Azimuth = (azimuth++ * 10) % 360;
            },
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)
        );
    }

    public MapViewModel(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Anchors = new ObservableList<IMapAnchor>();
        AnchorsView = Anchors.ToNotifyCollectionChangedSlim();
        Widgets = new ObservableList<IMapWidget>();
        WidgetsView = Widgets.ToNotifyCollectionChangedSlim();
        SelectedAnchor = new BindableReactiveProperty<IMapAnchor?>();
    }

    public NotifyCollectionChangedSynchronizedViewList<IMapWidget> WidgetsView { get; }

    public ObservableList<IMapWidget> Widgets { get; }

    public NotifyCollectionChangedSynchronizedViewList<IMapAnchor> AnchorsView { get; }

    public ObservableList<IMapAnchor> Anchors { get; }

    public BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        var anchor = AnchorsView.FirstOrDefault(x => x.Id == id);
        if (anchor != null)
        {
            SelectedAnchor.Value = anchor;
            return ValueTask.FromResult<IRoutable>(anchor);
        }

        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var item in AnchorsView)
        {
            yield return item;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            WidgetsView.Dispose();
            AnchorsView.Dispose();
        }

        base.Dispose(disposing);
    }
}
