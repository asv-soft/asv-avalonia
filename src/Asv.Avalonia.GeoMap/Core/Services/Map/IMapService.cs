using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapService
{
    public SynchronizedReactiveProperty<MapModeType> Mode { get; }
    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
    public SynchronizedReactiveProperty<int> MinZoom { get; }
    public SynchronizedReactiveProperty<int> MaxZoom { get; }
    public SynchronizedReactiveProperty<ITileProvider> CurrentProvider { get; }
    public IReadOnlyList<ITileProvider> AvailableProviders { get; }
}
