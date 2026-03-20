using R3;

namespace Asv.Avalonia.GeoMap;

public class NullZoomService : IZoomService
{
    public static IZoomService Instance { get; } = new NullZoomService();

    public SynchronizedReactiveProperty<int> MinZoom { get; } = new(IZoomService.MinZoomLevel);
    public SynchronizedReactiveProperty<int> MaxZoom { get; } = new(IZoomService.MaxZoomLevel);
}
