namespace Asv.Avalonia.Map;

public interface ITileProvider
{
    string? GetTileUrl(TilePosition position);
    int TileSize { get; }
}

public class EmptyTileProvider : ITileProvider
{
    public static ITileProvider Instance { get; } = new EmptyTileProvider();

    public string? GetTileUrl(TilePosition position)
    {
        return null;
    }

    public int TileSize => 256;
}
