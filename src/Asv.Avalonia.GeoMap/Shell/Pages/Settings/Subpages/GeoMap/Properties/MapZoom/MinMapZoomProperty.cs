using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MinMapZoomProperty : MapZoomPropertyBase
{
    public const string ViewModelId = "map-min-zoom";

    public MinMapZoomProperty()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MinMapZoomProperty(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId, mapService.MinZoom, loggerFactory)
    {
        Header = RS.SettingsGeoMapView_MinZoomProperty_Title;
        Description = RS.SettingsGeoMapView_MinZoomProperty_Description;
        Icon = MaterialIconKind.MagnifyMinusOutline;
        IconColor = AsvColorKind.Info4;

        mapService
            .MaxZoom.Select(maxZoom =>
                Enumerable.Range(IZoomService.MinZoomLevel, maxZoom - IZoomService.MinZoomLevel + 1)
            )
            .Subscribe(SetAvailableValues)
            .DisposeItWith(Disposable);
        SetAvailableValues(
            Enumerable.Range(
                IZoomService.MinZoomLevel,
                mapService.MaxZoom.Value - IZoomService.MinZoomLevel + 1
            )
        );
    }
}
