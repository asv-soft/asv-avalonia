using R3;

namespace Asv.Avalonia.GeoMap;

public interface IZoomService
{
    const int MinZoomLevel = 1;
    const int MaxZoomLevel = 19;

    SynchronizedReactiveProperty<int> MinZoom { get; }
    SynchronizedReactiveProperty<int> MaxZoom { get; }
}
