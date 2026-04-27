namespace Asv.Avalonia.GeoMap;

public class EmptyTileProvider : ITileProvider
{
    public static ITileProvider Instance { get; } = new EmptyTileProvider();

    public const string Id = "Empty";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.EmptyTileProvider_Info_Name,
        Group = TileProviderGroup.Other,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public int TileSize => 256;

    public Task<Tile?> DownloadAsync(TileKey key, CancellationToken ct)
    {
        return Task.FromResult<Tile?>(null);
    }
}
