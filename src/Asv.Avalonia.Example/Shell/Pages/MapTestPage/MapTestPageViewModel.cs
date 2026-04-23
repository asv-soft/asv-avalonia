using System;
using System.Collections.Generic;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

public class MapTestPageViewModel : PageViewModel<MapTestPageViewModel>
{
    public const string PageId = "map_test";
    public const MaterialIconKind PageIcon = MaterialIconKind.TestTube;
    private const int PlaneTrailLength = 100;
    private const double InfinityRadius = 750.0;
    private const double InfinityPhaseStep = Math.PI / 90.0;

    private readonly ILoggerFactory _loggerFactory;
    private readonly GeoPoint _centerPoint;

    public MapTestPageViewModel()
        : this(
            DesignTime.CommandService,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService,
            NullMapService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapTestPageViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensionService,
        IMapService mapService
    )
        : base(PageId, cmd, loggerFactory, dialogService, extensionService)
    {
        Title = RS.MapTestPageViewModel_Title;
        Icon = PageIcon;

        IsHeavyPolygonVisible = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);

        TileProviderSelectorViewModel = new TileProviderSelectorViewModel(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        MapViewModel = new MapViewModel("Map", loggerFactory, mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        MapViewModel.Anchors.SetRoutableParent(this).DisposeItWith(Disposable);

        _centerPoint = MapViewModel.CenterMap.Value;
        _loggerFactory = loggerFactory;

        CreateEditableAnchorCircle(pointCount: 36, radiusMeters: 1000);
        CreateRotationTestAnchors();
        CreateAnimatedPlane();

        IsHeavyPolygonVisible
            .ObserveOnUIThreadDispatcher()
            .Subscribe(visible =>
            {
                if (visible)
                {
                    CreateHeavyPolygon();
                }
                else
                {
                    RemoveHeavyPolygon();
                }
            })
            .DisposeItWith(Disposable);
    }

    public TileProviderSelectorViewModel TileProviderSelectorViewModel { get; }
    public MapViewModel MapViewModel { get; }
    public BindableReactiveProperty<bool> IsHeavyPolygonVisible { get; }

    #region Anchors

    private void CreateEditableAnchorCircle(int pointCount, double radiusMeters)
    {
        var path = new MapAnchor<IMapAnchor>("editable-anchor-path", _loggerFactory)
        {
            IsVisible = true,
            IsPolygonClosed = true,
            PolygonFill = SolidColorBrush.Parse("#80FFEBCD"),
        };
        MapViewModel.Anchors.Add(path);

        for (var i = 0; i < pointCount; i++)
        {
            var anchor = new MapAnchor<IMapAnchor>($"editable-anchor-{i}", _loggerFactory)
            {
                Icon = MaterialIconKind.MapMarker,
                Title = string.Format(RS.MapTestPageViewModel_Anchor_Editable, i),
                IsAnnotationVisible = false,
                CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0),
                Location = _centerPoint.RadialPoint(radiusMeters, 360.0 / pointCount * i),
            };
            MapViewModel.Anchors.Add(anchor);

            path.Polygon.Add(anchor.Location);
            var index = i;
            anchor
                .ObservePropertyChanged(x => x.Location)
                .Subscribe(location => path.Polygon[index] = location)
                .DisposeItWith(Disposable);
        }
    }

    private void CreateRotationTestAnchors()
    {
        // long annotation text
        AddAnchor(
            "long-text",
            MaterialIconKind.InformationOutline,
            RS.MapTestPageViewModel_Anchor_LongText,
            _centerPoint.RadialPoint(400, 0),
            centerY: new VerticalOffset(VerticalOffsetEnum.Bottom, 0)
        );

        // cluster of close anchors
        for (int i = 0; i < 5; i++)
        {
            AddAnchor(
                $"cluster-{i}",
                MaterialIconKind.CircleSmall,
                string.Format(RS.MapTestPageViewModel_Anchor_Cluster, i),
                _centerPoint.RadialPoint(50 + (i * 20), 45 + (i * 5))
            );
        }

        // edge anchors
        foreach (
            var (name, bearing) in new[]
            {
                ("North", 0.0),
                ("East", 90.0),
                ("South", 180.0),
                ("West", 270.0),
            }
        )
        {
            AddAnchor(
                $"edge-{name.ToLowerInvariant()}",
                MaterialIconKind.ArrowAll,
                string.Format(RS.MapTestPageViewModel_Anchor_Edge, name),
                _centerPoint.RadialPoint(2000, bearing)
            );
        }

        // different CenterX/CenterY offsets
        var offsetConfigs = new (string Name, HorizontalOffset Hx, VerticalOffset Vy)[]
        {
            ("TopLeft", new(HorizontalOffsetEnum.Left, 0), new(VerticalOffsetEnum.Top, 0)),
            ("TopRight", new(HorizontalOffsetEnum.Right, 0), new(VerticalOffsetEnum.Top, 0)),
            ("BottomLeft", new(HorizontalOffsetEnum.Left, 0), new(VerticalOffsetEnum.Bottom, 0)),
            ("BottomRight", new(HorizontalOffsetEnum.Right, 0), new(VerticalOffsetEnum.Bottom, 0)),
            (
                "CenterOffset20",
                new(HorizontalOffsetEnum.Center, 20),
                new(VerticalOffsetEnum.Center, 20)
            ),
        };
        for (var i = 0; i < offsetConfigs.Length; i++)
        {
            var (name, hx, vy) = offsetConfigs[i];
            AddAnchor(
                $"offset-{name.ToLowerInvariant()}",
                MaterialIconKind.CrosshairsGps,
                string.Format(RS.MapTestPageViewModel_Anchor_Offset, name),
                _centerPoint.RadialPoint(600, 72.0 * i),
                centerX: hx,
                centerY: vy
            );
        }

        // UseMapRotation comparison
        AddAnchor(
            "rotate-with-map",
            MaterialIconKind.Rotate3dVariant,
            RS.MapTestPageViewModel_Anchor_RotatesWithMap,
            _centerPoint.RadialPoint(500, 135),
            useMapRotation: true,
            azimuth: 45
        );

        AddAnchor(
            "no-map-rotation",
            MaterialIconKind.Rotate3dVariant,
            RS.MapTestPageViewModel_Anchor_NoMapRotation,
            _centerPoint.RadialPoint(500, 160),
            useMapRotation: false,
            azimuth: 45
        );

        // anchor with extreme azimuth values
        AddAnchor(
            "extreme-azimuth",
            MaterialIconKind.CompassOutline,
            RS.MapTestPageViewModel_Anchor_ExtremeAzimuth,
            _centerPoint.RadialPoint(500, 200),
            useMapRotation: true,
            azimuth: 359.9
        );

        // anchor at exact center
        AddAnchor(
            "at-center",
            MaterialIconKind.Bullseye,
            RS.MapTestPageViewModel_Anchor_Center,
            _centerPoint
        );
    }

    private MapAnchor<IMapAnchor> AddAnchor(
        string id,
        MaterialIconKind icon,
        string title,
        GeoPoint location,
        HorizontalOffset? centerX = null,
        VerticalOffset? centerY = null,
        bool useMapRotation = false,
        double? azimuth = null
    )
    {
        var anchor = new MapAnchor<IMapAnchor>(id, _loggerFactory)
        {
            Icon = icon,
            Title = title,
            IsReadOnly = true,
            IsAnnotationVisible = true,
            Location = location,
            CenterX = centerX ?? new HorizontalOffset(HorizontalOffsetEnum.Center, 0),
            CenterY = centerY ?? new VerticalOffset(VerticalOffsetEnum.Center, 0),
            UseMapRotation = useMapRotation,
            Azimuth = azimuth ?? 0,
        };

        MapViewModel.Anchors.Add(anchor);
        return anchor;
    }

    #endregion

    #region Plane

    private void CreateAnimatedPlane()
    {
        var plane = AddAnchor(
            "plane",
            MaterialIconKind.Navigation,
            RS.MapTestPageViewModel_Anchor_Plane,
            _centerPoint,
            useMapRotation: true
        );

        var planeTrail = new MapAnchor<IMapAnchor>("plane-trail", _loggerFactory)
        {
            IsVisible = false,
            IsPolygonClosed = false,
            PolygonPen = new Pen(SolidColorBrush.Parse("#FF1E88E5"), 2),
        };
        MapViewModel.Anchors.Add(planeTrail);

        var phase = 0.0;
        UpdatePlanePose(plane, _centerPoint, phase, planeTrail);

        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(100))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                phase += InfinityPhaseStep;
                UpdatePlanePose(plane, _centerPoint, phase, planeTrail);
            })
            .DisposeItWith(Disposable);
    }

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
        plane.Title = string.Format(
            RS.MapTestPageViewModel_Anchor_PlaneAzimuth,
            $"{NormalizeAngle(plane.Azimuth):F0}"
        );

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

    #endregion

    #region Heavy polygon

    private const string HeavyPolygonAnchorId = "heavy-polygon";
    private const int HeavyPolygonPointCount = 100_000;

    private void CreateHeavyPolygon()
    {
        RemoveHeavyPolygon();

        var anchor = new MapAnchor<IMapAnchor>(HeavyPolygonAnchorId, _loggerFactory)
        {
            IsVisible = true,
            IsPolygonClosed = true,
            PolygonPen = new Pen(SolidColorBrush.Parse("#FF4CAF50"), 1),
            PolygonFill = SolidColorBrush.Parse("#404CAF50"),
        };

        var polygonCenter = _centerPoint.RadialPoint(5000, 315);
        const double radius = 10000.0;

        for (var i = 0; i < HeavyPolygonPointCount; i++)
        {
            var bearing = 360.0 * i / HeavyPolygonPointCount;
            anchor.Polygon.Add(polygonCenter.RadialPoint(radius, bearing));
        }

        MapViewModel.Anchors.Add(anchor);
    }

    private void RemoveHeavyPolygon()
    {
        for (var i = MapViewModel.Anchors.Count - 1; i >= 0; i--)
        {
            if (MapViewModel.Anchors[i].Id == HeavyPolygonAnchorId)
            {
                MapViewModel.Anchors.RemoveAt(i);
            }
        }
    }

    #endregion

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return TileProviderSelectorViewModel;
        yield return MapViewModel;
    }

    protected override void AfterLoadExtensions() { }
}
