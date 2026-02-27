using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface ITileLoader
{
    void GetBitmap(TileKey key, Action<Bitmap> onLoaded);
    Observable<TileKey> OnLoaded { get; }
    ReactiveProperty<IBrush> EmptyTileBrush { get; }
}
