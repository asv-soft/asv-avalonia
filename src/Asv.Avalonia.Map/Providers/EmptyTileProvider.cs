namespace Asv.Avalonia.Map;

public class EmptyTileProvider : ITileProvider
{
    public static ITileProvider Instance { get; } = new EmptyTileProvider();

    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TilePosition position)
    {
        return null;
    }

    public int TileSize => 256;
}