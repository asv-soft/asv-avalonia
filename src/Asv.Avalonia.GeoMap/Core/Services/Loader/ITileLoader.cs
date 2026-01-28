using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface ITileLoader
{
    Bitmap this[TileKey key] { get; }
    Observable<TileKey> OnLoaded { get; }
    ReactiveProperty<IBrush> EmptyTileBrush { get; }
}
