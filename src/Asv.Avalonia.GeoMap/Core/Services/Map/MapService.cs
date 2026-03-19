using Asv.Common;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapService : AsyncDisposableOnce, IMapService
{
    private readonly ITileLoader _tileLoader;
    private readonly ITileProviderService _tileProviderService;

    public MapService(ITileLoader tileLoader, ITileProviderService tileProviderService)
    {
        ArgumentNullException.ThrowIfNull(tileLoader);
        ArgumentNullException.ThrowIfNull(tileProviderService);

        _tileLoader = tileLoader;
        _tileProviderService = tileProviderService;
    }

    public SynchronizedReactiveProperty<MapModeType> Mode => _tileLoader.CurrentMapMode;
    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush => _tileLoader.EmptyTileBrush;

    public SynchronizedReactiveProperty<ITileProvider> CurrentProvider =>
        _tileProviderService.CurrentProvider;
    public IReadOnlyList<ITileProvider> AvailableProviders =>
        _tileProviderService.AvailableProviders;
}
