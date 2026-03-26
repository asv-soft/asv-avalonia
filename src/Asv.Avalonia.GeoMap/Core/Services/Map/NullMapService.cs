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
        CurrentProvider = new SynchronizedReactiveProperty<ITileProvider>(
            EmptyTileProvider.Instance
        );
        AvailableProviders = [EmptyTileProvider.Instance];
        MinZoom = new SynchronizedReactiveProperty<int>(IZoomService.MinZoomLevel);
        MaxZoom = new SynchronizedReactiveProperty<int>(IZoomService.MaxZoomLevel);
    }

    public SynchronizedReactiveProperty<MapModeType> Mode { get; }
    public SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
    public SynchronizedReactiveProperty<int> MinZoom { get; }
    public SynchronizedReactiveProperty<int> MaxZoom { get; }
    public SynchronizedReactiveProperty<ITileProvider> CurrentProvider { get; }
    public IReadOnlyList<ITileProvider> AvailableProviders { get; }

    public void SetProviderApiKey(string providerId, string? apiKey) { }

    public string? GetProviderApiKey(string providerId) => null;
}
