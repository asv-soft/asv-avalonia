using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MinMapZoomProperty : MapZoomPropertyBase
{
    public const string ViewModelId = "map.min-zoom";

    public MinMapZoomProperty()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MinMapZoomProperty(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId, mapService.MinZoom, ChangeMinZoomCommand.Id, loggerFactory)
    {
        Items = mapService
            .MaxZoom.Select(maxZoom =>
                Enumerable.Range(IZoomService.MinZoomLevel, maxZoom - IZoomService.MinZoomLevel + 1)
            )
            .ToReadOnlyBindableReactiveProperty(
                Enumerable.Range(
                    IZoomService.MinZoomLevel,
                    mapService.MaxZoom.Value - IZoomService.MinZoomLevel + 1
                )
            )
            .DisposeItWith(Disposable);
    }

    public IReadOnlyBindableReactiveProperty<IEnumerable<int>> Items { get; }
}
