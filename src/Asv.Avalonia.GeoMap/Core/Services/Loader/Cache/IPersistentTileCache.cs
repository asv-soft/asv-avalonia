namespace Asv.Avalonia.GeoMap;

public interface IPersistentTileCache
{
    bool Contains(TileKey key);

    /// <summary>
    /// Persists the tile before returning. The caller retains ownership of the tile.
    /// </summary>
    ValueTask SaveAsync(Tile tile, CancellationToken cancellationToken = default);
}
