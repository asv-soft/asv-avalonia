namespace Asv.Avalonia.Map;

public interface ITileProvider
{
    string? GetTileUrl(TileKey key);
    int TileSize { get; }
}

public class EmptyTileProvider : ITileProvider
{
    public static ITileProvider Instance { get; } = new EmptyTileProvider();

    public string? GetTileUrl(TileKey key)
    {
        return null;
    }

    public int TileSize => 256;
}
