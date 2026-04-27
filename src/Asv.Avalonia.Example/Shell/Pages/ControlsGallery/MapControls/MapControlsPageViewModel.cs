using System;
using System.Collections.Generic;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public class MapControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "map_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.Map;
    private const int PlaneTrailLength = 100;
    private const double InfinityRadius = 750.0;
    private const double InfinityPhaseStep = Math.PI / 90.0;

    public MapControlsPageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance, 
            DesignTime.LoggerFactory, 
            NullMapService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapControlsPageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        ILoggerFactory loggerFactory, 
        IMapService mapService)
        : base(PageId, context)
    {
        TileProviderSelectorViewModel = new TileProviderSelectorViewModel(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        MapViewModel = new MapViewModel("Map", mapService)
            .DisposeItWith(Disposable)
            .SetRoutableParent(this);

        MapViewModel.Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        MapViewModel.Anchors.SetRoutableParent(this).DisposeItWith(Disposable);

        var centerPoint = MapViewModel.CenterMap.Value;
        var pointCount = 36;

        var path = new MapAnchor<IMapAnchor>("editanle-anchor-path");
        path.IsVisible = true;
        path.IsPolygonClosed = true;
        path.PolygonPen = new Pen(new SolidColorBrush(Colors.Black), 2);
        path.PolygonFill = SolidColorBrush.Parse("#80FFEBCD");
        MapViewModel.Anchors.Add(path);
        for (int i = 0; i < pointCount; i++)
        {
            var anchor = new MapAnchor<IMapAnchor>($"editable-anchor-{i}");
            anchor.Icon = MaterialIconKind.MapMarker;
            anchor.Title = $"Anchor {i}";
            anchor.IsAnnotationVisible = false;
            anchor.CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0);
            anchor.Location = centerPoint.RadialPoint(1000, 360.0 / pointCount * i);
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
        var plane = new MapAnchor<IMapAnchor>("plane");
        plane.Icon = MaterialIconKind.Navigation;
        plane.Title = "Plane";
        plane.IsReadOnly = true;
        plane.IsAnnotationVisible = true;
        plane.CenterX = new HorizontalOffset(HorizontalOffsetEnum.Center, 0);
        plane.CenterY = new VerticalOffset(VerticalOffsetEnum.Center, 0);
        plane.UseMapRotation = true;
        MapViewModel.Anchors.Add(plane);

        var planeTrail = new MapAnchor<IMapAnchor>("plane-trail");
        planeTrail.IsVisible = false;
        plane.IsReadOnly = true;
        planeTrail.IsPolygonClosed = false;
        MapViewModel.Anchors.Add(planeTrail);

        var phase = 0.0;
        UpdatePlanePose(plane, centerPoint, phase, planeTrail);

        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(100))
            .Subscribe(_ =>
            {
                phase += InfinityPhaseStep;
                UpdatePlanePose(plane, centerPoint, phase, planeTrail);
            })
            .DisposeItWith(Disposable);
    }

    public override IEnumerable<IViewModel> GetChildren()
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

    private static void UpdatePlanePose(
        MapAnchor<IMapAnchor> plane,
        GeoPoint centerPoint,
        double phase,
        MapAnchor<IMapAnchor> planeTrail
    )
    {
        var sinPhase = Math.Sin(phase);
        var cosPhase = Math.Cos(phase);

        var eastOffset = InfinityRadius * sinPhase;
        var northOffset = InfinityRadius * sinPhase * cosPhase;

        var eastVelocity = InfinityRadius * cosPhase;
        var northVelocity = InfinityRadius * ((cosPhase * cosPhase) - (sinPhase * sinPhase));

        plane.Location = OffsetFromCenter(centerPoint, eastOffset, northOffset);
        plane.Azimuth = Math.Atan2(eastVelocity, northVelocity) * 180.0 / Math.PI;
        plane.Title = $"Plane {NormalizeAngle(plane.Azimuth):F0}°";

        planeTrail.Polygon.Add(plane.Location);
        while (planeTrail.Polygon.Count > PlaneTrailLength)
        {
            planeTrail.Polygon.RemoveAt(0);
        }
    }

    private static GeoPoint OffsetFromCenter(
        GeoPoint centerPoint,
        double eastOffset,
        double northOffset
    )
    {
        var distance = Math.Sqrt((eastOffset * eastOffset) + (northOffset * northOffset));
        if (distance <= double.Epsilon)
        {
            return centerPoint;
        }

        var azimuth = Math.Atan2(eastOffset, northOffset) * 180.0 / Math.PI;
        return centerPoint.RadialPoint(distance, azimuth);
    }

    private static double NormalizeAngle(double angle)
    {
        var normalized = angle % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }
}
