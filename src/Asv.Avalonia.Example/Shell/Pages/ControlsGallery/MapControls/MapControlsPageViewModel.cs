using System;
using System.Collections.Generic;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.IO;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public class MapControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "map_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.Map;

    public MapControlsPageViewModel()
        : this(DesignTime.LoggerFactory, NullMapService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapControlsPageViewModel(ILoggerFactory loggerFactory, IMapService mapService)
        : base(PageId, loggerFactory)
    {
        TileProviderSelectorViewModel = new TileProviderSelectorViewModel(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        MapViewModel = new MapViewModel("Map", loggerFactory, mapService)
            .DisposeItWith(Disposable)
            .SetRoutableParent(this);

        MapViewModel.Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        MapViewModel.Anchors.SetRoutableParent(this).DisposeItWith(Disposable);

        Observable
            .Timer(TimeSpan.FromSeconds(1))
            .Subscribe(x =>
            {
                var centerPoint = MapViewModel.CenterMap.Value;
                var pointCount = 36;

                var path = new MapAnchor<IMapAnchor>("editanle-anchor-path", loggerFactory);
                path.IsVisible = true;
                path.IsPolygonClosed = true;
                path.PolygonFill = SolidColorBrush.Parse("#80FFEBCD");
                MapViewModel.Anchors.Add(path);
                for (int i = 0; i < pointCount; i++)
                {
                    var anchor = new MapAnchor<IMapAnchor>($"editable-anchor-{i}", loggerFactory);
                    anchor.Icon = MaterialIconKind.MapMarker;
                    anchor.Title = string.Empty;
                    anchor.IsAnnotationVisible = false;
                    anchor.CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0);
                    anchor.Location = centerPoint.RadialPoint(1000, 360 / pointCount * i);
                    MapViewModel.Anchors.Add(anchor);

                    path.Polygon.Add(anchor.Location);
                    var i1 = i;
                    anchor
                        .ObservePropertyChanged(x => x.Location)
                        .Subscribe(location =>
                        {
                            path.Polygon[i1] = location;
                        })
                        .DisposeItWith(Disposable);
                }
            });
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return TileProviderSelectorViewModel;
        yield return MapViewModel;

        foreach (var child in base.GetChildren())
        {
            yield return child;
        }
    }

    public TileProviderSelectorViewModel TileProviderSelectorViewModel { get; }
    public MapViewModel MapViewModel { get; }
}
