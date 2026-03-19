using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MaxMapZoomProperty : MapZoomPropertyBase
{
    public const string ViewModelId = "map.max-zoom";

    public MaxMapZoomProperty()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MaxMapZoomProperty(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId, mapService.MaxZoom, ChangeMaxZoomCommand.Id, loggerFactory)
    {
        Items = mapService
            .MinZoom.Select(minZoom =>
                Enumerable.Range(minZoom, IZoomService.MaxZoomLevel - minZoom + 1)
            )
            .ToReadOnlyBindableReactiveProperty(
                Enumerable.Range(
                    mapService.MinZoom.Value,
                    IZoomService.MaxZoomLevel - mapService.MinZoom.Value + 1
                )
            )
            .DisposeItWith(Disposable);
    }

    public IReadOnlyBindableReactiveProperty<IEnumerable<int>> Items { get; }
}
