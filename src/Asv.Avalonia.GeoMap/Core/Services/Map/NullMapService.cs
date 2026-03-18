using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public class NullMapService : IMapService
{
    public static IMapService Instance { get; } = new NullMapService();

    public NullMapService()
    {
        Mode = new SynchronizedReactiveProperty<MapModeType>(MapModeType.Mixed);
        EmptyTileBrush = new SynchronizedReactiveProperty<IBrush>(
            Brush.Parse(new TileLoaderConfig().EmptyTileBrush)
        );
    }

    public SynchronizedReactiveProperty<MapModeType> Mode { get; }
    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
}
