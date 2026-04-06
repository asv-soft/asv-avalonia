namespace Asv.Avalonia.GeoMap;

public class ThunderforestOutdoorsTileProvider : IProtectedTileProvider
{
    public const string Id = "ThunderforestOutdoors";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ThunderforestOutdoorsTileProvider_Info_Name,
        Group = TileProviderGroup.Thunderforest,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://api.thunderforest.com/outdoors/{key.Zoom}/{key.X}/{key.Y}.png?apikey={ApiKey}";
    }

    public int TileSize => 256;
}
