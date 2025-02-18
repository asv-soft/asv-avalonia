namespace Asv.Avalonia.Map;

public interface ITileProvider
{
    string Id { get; }
    IMapProjection Projection { get; }
    string? GetTileUrl(TilePosition position);
    int TileSize { get; }
}
