using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            DesignTime.UnitService,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsGeoMapViewModel(
        ITreeSubPageContext<ISettingsPage> pageContext,
        IMapService mapService,
        IDialogService dialogService,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        IAppPath? appPath = null,
        IOptions<FileSystemCacheConfig>? cacheConfig = null
    )
        : base(PageId, pageContext)
    {
        UnitService = unitService;
        LoggerFactory = loggerFactory;

        TileProviderSelectorViewModel = new TileProviderSelectorViewModel(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Editor = new ExtendedPropertyEditorViewModel("editor")
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

        if (mapService.Input is { } input)
        {
            MapInputGestures = Enum.GetValues<MapInputAction>()
                .Select(action => new MapInputGestureProperty(action, input))
                .ToArray();
            foreach (var gesture in MapInputGestures)
            {
                Editor.ItemsSource.Add(gesture);
            }

            ResetMapInput = new PropertyButtonViewModel(
                "reset-map-input",
                cancel =>
                {
                    cancel.ThrowIfCancellationRequested();
                    input.Reset();
                    foreach (var gesture in MapInputGestures)
                    {
                        gesture.RefreshFromService();
                    }
                    return ValueTask.CompletedTask;
                }
            )
            {
                Header = MapInputStrings.Get("Reset_Header"),
                ShortHeader = MapInputStrings.Get("Reset_Button"),
                Description = MapInputStrings.Get("Reset_Description"),
                Icon = MaterialIconKind.Restore,
            };
            Editor.ItemsSource.Add(ResetMapInput);
        }

        var mapFolder = appPath?.GetAppPathFolder(
            cacheConfig?.Value.FolderPath ?? new FileSystemCacheConfig().FolderPath
        ) ?? Path.GetFullPath(Path.Combine("data", "map"));
        var openMapFolderHeader = RS.ResourceManager.GetString(
            "SettingsGeoMapViewModel_OpenMapFolder_Header", RS.Culture
        )!;
        OpenMapFolder = new PropertyButtonViewModel(
            "open-map-folder",
            async cancel =>
            {
                cancel.ThrowIfCancellationRequested();
                await dialogService.ShowObserveFolderDialogAsync(openMapFolderHeader, mapFolder);
            }
        )
        {
            Header = openMapFolderHeader,
            ShortHeader = RS.ResourceManager.GetString(
                "SettingsGeoMapViewModel_OpenMapFolder_Button", RS.Culture
            )!,
            Description = mapFolder,
            Icon = MaterialIconKind.FolderOpen,
        };
        Editor.ItemsSource.Add(OpenMapFolder);

        MapPreview = new MapViewModel("preview", mapService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MapPreview.CenterMap.Value = new GeoPoint(56.8389, 60.6057, 0);
        MapPreview.Zoom.Value = ClampZoom(13, mapService);
        CreatePreviewPath();

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

    public IReadOnlyList<MapInputGestureProperty> MapInputGestures { get; } = [];
    public PropertyButtonViewModel? ResetMapInput { get; }
    public ExtendedPropertyEditorViewModel Editor { get; }
    public TileProviderSelectorViewModel TileProviderSelectorViewModel { get; }
    public MapViewModel MapPreview { get; }
    public string MapPreviewHeader => RS.SettingsGeoMapView_MapPreview_Title;
    public string MapPreviewDescription => RS.SettingsGeoMapView_MapPreview_Description;
    public IUnitService UnitService { get; }
    public ILoggerFactory LoggerFactory { get; }
    public PropertyButtonViewModel OpenMapFolder { get; }
    public MapProviderProperty MapProvider { get; }
    public MapModeProperty MapMode { get; }
    public MinMapZoomProperty MinMapZoom { get; }
    public MaxMapZoomProperty MaxMapZoom { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Editor;
        yield return TileProviderSelectorViewModel;
        yield return MapPreview;
    }

    private void CreatePreviewPath()
    {
        MapPreview.ShowAltitudes.Value = true;
        MapPreview.ShowPolygonAltitudes.Value = true;
        MapPreview.AltitudePixelsPerMeter.Value = 0.1;

        var path = new MapAnchor("preview-path")
        {
            Icon = null,
            IsReadOnly = true,
            IsAnnotationVisible = false,
            IsPolygonClosed = false,
            PolygonPen = new Pen(Brushes.DodgerBlue, 2),
        };
        MapPreview.Anchors.Add(path);

        double[] heights = [100, 200, 350, 250, 150];
        var center = MapPreview.CenterMap.Value;
        var headerFormat = RS.ResourceManager.GetString(
            "SettingsGeoMapView_MapPreview_PointHeight", RS.Culture
        )!;
        for (var i = 0; i < heights.Length; i++)
        {
            var location = new GeoPoint(
                center.Latitude,
                center.Longitude + ((i - 2) * 0.01),
                heights[i]
            );
            var anchor = new MapAnchor($"preview-point-{i + 1}", location: location)
            {
                Header = string.Format(headerFormat, i + 1, heights[i]),
                Icon = MaterialIconKind.MapMarker,
                IconSize = 24,
                IconColor = AsvColorKind.Info7,
                CenterX = HorizontalOffset.Default,
                CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0),
                IsReadOnly = false,
                CanDragWithoutModifier = false,
                IsAnnotationVisible = false,
            };
            MapPreview.Anchors.Add(anchor);
            path.Polygon.Add(location);
            var index = i;
            anchor.ObservePropertyChanged(point => point.Location)
                .Subscribe(point => path.Polygon[index] = point)
                .DisposeItWith(Disposable);
        }
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
