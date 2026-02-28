using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface ITileLoader
{
    Observable<TileKey> OnLoaded { get; }
    ReactiveProperty<IBrush> EmptyTileBrush { get; }
    void Render(DrawingContext context, double x, double y, TileKey key);
}
