using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Mavlink;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class FlightPageViewModel : PageViewModel<IFlightModeContext>, IFlightModeContext
{
    public const string PageId = "Flight";

    public FlightPageViewModel()
        : this(DesignTime.CommandService)
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

    [ImportingConstructor]
    public FlightPageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
        Title.Value = "Flight";
        Anchors = [];
        AnchorsView = Anchors.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Widgets = [];
        WidgetsView = Widgets.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        SelectedAnchor = new BindableReactiveProperty<IMapAnchor?>().DisposeItWith(Disposable);

        var drone = new MapAnchor("1")
        {
            Icon = MaterialIconKind.Navigation,
            Location = new GeoPoint(53, 53, 100),
        };
        var drone2 = new MapAnchor("1")
        {
            Icon = MaterialIconKind.Navigation,
            Location = new GeoPoint(53, 53, 100),
        };
        Anchors.Add(drone);
        Anchors.Add(drone2);
        var azimuth = 0;
        TimeProvider.System.CreateTimer(
            x =>
            {
                drone.Azimuth = (azimuth++ * 10) % 360;
                drone.Title = $"{drone.Azimuth} deg";
                drone.Location = drone.Location.RadialPoint(100, drone.Azimuth);
            },
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)
        );
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

        foreach (var widget in WidgetsView)
        {
            yield return widget;
        }
    }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }

    public override IExportInfo Source => SystemModule.Instance;
}
