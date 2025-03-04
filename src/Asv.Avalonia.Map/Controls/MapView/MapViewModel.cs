using System.Collections;
using Asv.Common;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Map;

public interface IMap : IRoutable
{
    ObservableList<IMapWidget> Widgets { get; }
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
}

public interface IMapWidget : IRoutable { }

public class MapViewModel : RoutableViewModel, IMap
{
    public MapViewModel()
        : this("id")
    {
        DesignTime.ThrowIfNotDesignMode();
        var drone = new MapAnchor("1")
        {
            Icon = MaterialIconKind.Navigation,
            Location = new GeoPoint(53, 53, 100),
        };
        Anchors.Add(drone);
        var azimuth = 0;
        TimeProvider.System.CreateTimer(
            x =>
            {
                drone.Azimuth = (azimuth++ * 10) % 360;
            },
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)
        );
    }

    public MapViewModel(string id)
        : base(id)
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

    public override ValueTask<IRoutable> Navigate(string id)
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
        throw new NotImplementedException();
    }
}
