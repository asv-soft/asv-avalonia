namespace Asv.Avalonia.GeoMap;

public class HereMapTileProvider : IProtectedTileProvider
{
    public const string Id = "HereMap";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.HereMapTileProvider_Info_Name,
        Group = TileProviderGroup.Here,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://maps.hereapi.com/v3/base/mc/{key.Zoom}/{key.X}/{key.Y}/png?apiKey={ApiKey}";
    }

    public int TileSize => 256;
}
