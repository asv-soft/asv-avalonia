using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MaxMapZoomProperty : MapZoomPropertyBase
{
    public const string ViewModelId = "map-max-zoom";

    public MaxMapZoomProperty()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MaxMapZoomProperty(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId, mapService.MaxZoom, loggerFactory)
    {
        Header = RS.SettingsGeoMapView_MaxZoomProperty_Title;
        Description = RS.SettingsGeoMapView_MaxZoomProperty_Description;
        Icon = MaterialIconKind.MagnifyPlusOutline;
        IconColor = AsvColorKind.Info5;

        mapService
            .MinZoom.Select(minZoom =>
                Enumerable.Range(minZoom, IZoomService.MaxZoomLevel - minZoom + 1)
            )
            .Subscribe(SetAvailableValues)
            .DisposeItWith(Disposable);
        SetAvailableValues(
            Enumerable.Range(
                mapService.MinZoom.Value,
                IZoomService.MaxZoomLevel - mapService.MinZoom.Value + 1
            )
        );
    }
}
