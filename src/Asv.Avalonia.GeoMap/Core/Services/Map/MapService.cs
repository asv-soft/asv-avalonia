using Asv.Common;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapService : AsyncDisposableOnce, IMapService
{
    private readonly ITileLoader _tileLoader;
    private readonly ITileProviderService _tileProviderService;
    private readonly IZoomService _zoomService;

    public MapService(
        ITileLoader tileLoader,
        IZoomService zoomService,
        ITileProviderService tileProviderService
    )
    {
        ArgumentNullException.ThrowIfNull(tileLoader);
        ArgumentNullException.ThrowIfNull(tileProviderService);
        ArgumentNullException.ThrowIfNull(zoomService);

        _tileLoader = tileLoader;
        _tileProviderService = tileProviderService;
        _zoomService = zoomService;
    }

    public SynchronizedReactiveProperty<MapModeType> Mode => _tileLoader.CurrentMapMode;
    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush => _tileLoader.EmptyTileBrush;

    public SynchronizedReactiveProperty<ITileProvider> CurrentProvider =>
        _tileProviderService.CurrentProvider;
    public IReadOnlyList<ITileProvider> AvailableProviders =>
        _tileProviderService.AvailableProviders;
    public SynchronizedReactiveProperty<int> MinZoom => _zoomService.MinZoom;
    public SynchronizedReactiveProperty<int> MaxZoom => _zoomService.MaxZoom;
}
