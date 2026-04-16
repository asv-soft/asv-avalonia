namespace Asv.Avalonia.GeoMap;

public class AzureMapsHybridTileProvider : IProtectedTileProvider
{
    public const string Id = "AzureMapsHybrid";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.AzureMapsHybridTileProvider_Info_Name,
        Group = TileProviderGroup.AzureMaps,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://atlas.microsoft.com/map/tile?api-version=2024-04-01&tilesetId=microsoft.base.hybrid.road&zoom={key.Zoom}&x={key.X}&y={key.Y}&subscription-key={ApiKey}";
    }

    public int TileSize => 256;
}
