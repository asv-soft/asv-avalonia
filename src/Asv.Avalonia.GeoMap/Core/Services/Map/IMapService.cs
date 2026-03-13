using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapService
{
    public SynchronizedReactiveProperty<MapModeType> Mode { get; }
    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
}
