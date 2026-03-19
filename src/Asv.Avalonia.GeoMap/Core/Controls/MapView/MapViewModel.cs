using Asv.Common;
using Asv.IO;
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
        : this(DesignTime.Id, DesignTime.LoggerFactory, NullMapService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        var drone = new MapAnchor<IMapAnchor>(DesignTime.Id, DesignTime.LoggerFactory)
        {
            Icon = MaterialIconKind.Navigation,
            Location = new GeoPoint(53, 53, 100),
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

    public MapViewModel(NavigationId id, ILoggerFactory loggerFactory, IMapService mapService)
        : base(id, loggerFactory)
    {
        Anchors = new ObservableList<IMapAnchor>();
        Anchors.SetRoutableParent(this).DisposeItWith(Disposable);
        Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        AnchorsView = Anchors.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        SelectedAnchor = new BindableReactiveProperty<IMapAnchor?>().DisposeItWith(Disposable);

        MinZoom = mapService
            .MinZoom.ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty(mapService.MinZoom.Value)
            .DisposeItWith(Disposable);
        MaxZoom = mapService
            .MaxZoom.ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty(mapService.MaxZoom.Value)
            .DisposeItWith(Disposable);

        CenterMap = new BindableReactiveProperty<GeoPoint>(
            new GeoPoint(53.0, 53.0, 0)
        ).DisposeItWith(Disposable);
        Zoom = new BindableReactiveProperty<int>(10).DisposeItWith(Disposable);

        CurrentProvider = mapService
            .CurrentProvider.ToReadOnlyBindableReactiveProperty<ITileProvider>()
            .DisposeItWith(Disposable);
    }

    public IReadOnlyBindableReactiveProperty<ITileProvider> CurrentProvider { get; }

    public NotifyCollectionChangedSynchronizedViewList<IMapAnchor> AnchorsView { get; }

    public ObservableList<IMapAnchor> Anchors { get; }

    public BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
    public BindableReactiveProperty<GeoPoint> CenterMap { get; }
    public BindableReactiveProperty<int> Zoom { get; }

    public IReadOnlyBindableReactiveProperty<int> MinZoom { get; }
    public IReadOnlyBindableReactiveProperty<int> MaxZoom { get; }

    public override IEnumerable<IRoutable> GetChildren()
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
