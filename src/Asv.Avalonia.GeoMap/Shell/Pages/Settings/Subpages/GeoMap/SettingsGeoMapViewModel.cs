using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class SettingsGeoMapViewModel : SettingsSubPage, ISettingsGeoMapSubPage
{
    public const string PageId = "geo-map";

    public SettingsGeoMapViewModel()
        : this(
            NullTreeSubPageContext<SettingsPageViewModel>.Instance,
            NullMapService.Instance,
            DesignTime.DialogService,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsGeoMapViewModel(
        ITreeSubPageContext<ISettingsPage> pageContext,
        IMapService mapService,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, pageContext)
    {
        Editor = new ExtendedPropertyEditorViewModel($"{PageId}.editor")
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        MapMode = new MapModeProperty(mapService, loggerFactory);
        MapProvider = new MapProviderProperty(mapService, dialogService, loggerFactory);
        MinMapZoom = new MinMapZoomProperty(mapService, loggerFactory);
        MaxMapZoom = new MaxMapZoomProperty(mapService, loggerFactory);

        Editor.ItemsSource.Add(MapMode);
        Editor.ItemsSource.Add(MapProvider);
        Editor.ItemsSource.Add(MinMapZoom);
        Editor.ItemsSource.Add(MaxMapZoom);

        MapPreview = new MapViewModel($"{PageId}.preview", mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MapPreview.CenterMap.Value = new GeoPoint(56.8389, 60.6057, 0);
        MapPreview.Zoom.Value = ClampZoom(10, mapService);
        MapPreview.Anchors.Add(
            new MapAnchor($"{PageId}.preview-anchor", location: MapPreview.CenterMap.Value)
            {
                Header = RS.SettingsGeoMapView_MapPreview_Anchor,
                Icon = MaterialIconKind.MapMarkerRadius,
                IconColor = AsvColorKind.Info7,
                CenterX = HorizontalOffset.Default,
                CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0),
                IsReadOnly = true,
                IsAnnotationVisible = true,
            }
        );

        mapService
            .MinZoom.Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => ClampPreviewZoom(mapService))
            .DisposeItWith(Disposable);
        mapService
            .MaxZoom.Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => ClampPreviewZoom(mapService))
            .DisposeItWith(Disposable);
    }

    public ExtendedPropertyEditorViewModel Editor { get; }
    public MapViewModel MapPreview { get; }
    public string MapPreviewHeader => RS.SettingsGeoMapView_MapPreview_Title;
    public string MapPreviewDescription => RS.SettingsGeoMapView_MapPreview_Description;
    public MapProviderProperty MapProvider { get; }
    public MapModeProperty MapMode { get; }
    public MinMapZoomProperty MinMapZoom { get; }
    public MaxMapZoomProperty MaxMapZoom { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Editor;
        yield return MapPreview;
    }

    private void ClampPreviewZoom(IMapService mapService)
    {
        MapPreview.Zoom.Value = ClampZoom(MapPreview.Zoom.Value, mapService);
    }

    private static int ClampZoom(int zoom, IMapService mapService)
    {
        return Math.Clamp(zoom, mapService.MinZoom.Value, mapService.MaxZoom.Value);
    }
}
