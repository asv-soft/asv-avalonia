namespace Asv.Avalonia.GeoMap;

public interface ITileProvider
{
    TileProviderInfo Info { get; }
    IMapProjection Projection { get; }
    string? GetTileUrl(TileKey key);
    int TileSize { get; }
}

public interface IProtectedTileProvider : ITileProvider
{
    string? ApiKey { get; set; }
}
