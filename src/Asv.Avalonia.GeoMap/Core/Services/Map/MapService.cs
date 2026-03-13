using Asv.Cfg;
using Asv.Common;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapService : AsyncDisposableOnce, IMapService
{
    private readonly ITileLoader _tileLoader;

    public MapService(ITileLoader tileLoader)
    {
        ArgumentNullException.ThrowIfNull(tileLoader);

        _tileLoader = tileLoader;
    }

    public SynchronizedReactiveProperty<MapModeType> Mode => _tileLoader.CurrentMapMode;
    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush => _tileLoader.EmptyTileBrush;
}
