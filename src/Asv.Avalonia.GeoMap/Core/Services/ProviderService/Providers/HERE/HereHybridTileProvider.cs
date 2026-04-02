namespace Asv.Avalonia.GeoMap;

public class HereHybridTileProvider : IProtectedTileProvider
{
    public const string Id = "HereHybrid";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.HereHybridTileProvider_Info_Name,
        Group = TileProviderGroup.Here,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://maps.hereapi.com/v3/base/mc/{key.Zoom}/{key.X}/{key.Y}/png?style=satellite.day&apiKey={ApiKey}";
    }

    public int TileSize => 256;
}
