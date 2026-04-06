namespace Asv.Avalonia.GeoMap;

public class HereSatelliteTileProvider : IProtectedTileProvider
{
    public const string Id = "HereSatellite";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.HereSatelliteTileProvider_Info_Name,
        Group = TileProviderGroup.Here,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://maps.hereapi.com/v3/satellite/mc/{key.Zoom}/{key.X}/{key.Y}/png?apiKey={ApiKey}";
    }

    public int TileSize => 256;
}
