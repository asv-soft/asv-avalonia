﻿using System;
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
public class FlightPageViewModel : PageViewModel<IFlightMode>, IFlightMode
{
    public const string PageId = "flight";
    public const MaterialIconKind PageIcon = MaterialIconKind.MapSearch;

    public FlightPageViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();
        var drone = new MapAnchor<IMapAnchor>("1")
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
        Widgets.Add(new UavWidgetViewModel { Header = "Device11" });
    }

    [ImportingConstructor]
    public FlightPageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
        Title.Value = "Flight";
        Icon.Value = PageIcon;
        Anchors = [];
        Anchors.SetRoutableParent(this, true).DisposeItWith(Disposable);
        AnchorsView = Anchors.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Widgets = [];
        Widgets.SetRoutableParent(this, true).DisposeItWith(Disposable);
        WidgetsView = Widgets.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        SelectedAnchor = new BindableReactiveProperty<IMapAnchor?>().DisposeItWith(Disposable);
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
