using Avalonia.Media.Imaging;

namespace Asv.Avalonia.Map;

public class TileCacheStatistic(
    long hits,
    long misses,
    long tileCount,
    long size,
    long capacitySize
)
{
    public static TileCacheStatistic Empty { get; } = new(0, 0, 0, 0, 0);
    public long Hits { get; } = hits;
    public long Misses { get; } = misses;
    public long TileCount { get; } = tileCount;
    public long Size { get; } = size;
    public long CapacitySize { get; } = capacitySize;

    public override string ToString()
    {
        return $"Hits: {Hits:N0}, Misses: {Misses:N0}, TileCount: {TileCount:N0}, Size: {Size:N0}, CapacitySize: {CapacitySize:N0}";
    }
}

public interface ITileCache
{
    Bitmap? this[TileKey key] { get; set; }
    TileCacheStatistic GetStatistic();
}
