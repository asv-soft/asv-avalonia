using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMap : IRoutable
{
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
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
        Anchors.SetRoutableParent(this).DisposeItWith(Disposable);
        Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        AnchorsView = Anchors.ToNotifyCollectionChangedSlim();
        SelectedAnchor = new BindableReactiveProperty<IMapAnchor?>().DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<IMapAnchor> AnchorsView { get; }

    public ObservableList<IMapAnchor> Anchors { get; }

    public BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }

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
            AnchorsView.Dispose();
        }

        base.Dispose(disposing);
    }
}
