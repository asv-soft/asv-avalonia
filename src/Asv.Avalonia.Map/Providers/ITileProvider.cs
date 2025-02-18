namespace Asv.Avalonia.Map;

public interface ITileProvider
{
    IMapProjection Projection { get; }
    string? GetTileUrl(TilePosition position);
    int TileSize { get; }
}
