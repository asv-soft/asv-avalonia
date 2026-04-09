namespace Asv.Avalonia.GeoMap;

public interface ITileProvider
{
    TileProviderInfo Info { get; }
    IMapProjection Projection { get; }
    int TileSize { get; }
    Task<Tile?> DownloadAsync(TileKey key, CancellationToken ct);
}

public interface IProtectedTileProvider : ITileProvider
{
    string? ApiKey { get; set; }
}
