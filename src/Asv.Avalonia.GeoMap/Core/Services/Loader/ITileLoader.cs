using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface ITileLoader
{
    Observable<TileKey> OnLoaded { get; }
    SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
    SynchronizedReactiveProperty<MapModeType> CurrentMapMode { get; }
    void Render(DrawingContext context, double x, double y, TileKey key);
}
