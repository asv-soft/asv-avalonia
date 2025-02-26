namespace Asv.Avalonia.Map;

public class EmptyTileProvider : ITileProvider
{
    public const string Id = "Empty";
    public static readonly TileProviderInfo StaticInfo = new(Id, "Empty");
    public static ITileProvider Instance { get; } = new EmptyTileProvider();
    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey position)
    {
        return null;
    }

    public int TileSize => 256;
}
