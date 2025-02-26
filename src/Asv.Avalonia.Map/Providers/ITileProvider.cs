namespace Asv.Avalonia.Map;

public class TileProviderInfo(string id, string name)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
}

public interface ITileProvider
{
    TileProviderInfo Info { get; }
    IMapProjection Projection { get; }
    string? GetTileUrl(TileKey key);
    int TileSize { get; }
}
