namespace Asv.Avalonia.GeoMap;

public class ThunderforestTransportDarkTileProvider : IProtectedTileProvider
{
    public const string Id = "ThunderforestTransportDark";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ThunderforestTransportDarkTileProvider_Info_Name,
        Group = TileProviderGroup.Thunderforest,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://api.thunderforest.com/transport-dark/{key.Zoom}/{key.X}/{key.Y}.png?apikey={ApiKey}";
    }

    public int TileSize => 256;
}
