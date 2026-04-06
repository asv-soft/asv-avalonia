namespace Asv.Avalonia.GeoMap;

public class ThunderforestCycleTileProvider : IProtectedTileProvider
{
    public const string Id = "ThunderforestCycle";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ThunderforestCycleTileProvider_Info_Name,
        Group = TileProviderGroup.Thunderforest,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://api.thunderforest.com/cycle/{key.Zoom}/{key.X}/{key.Y}.png?apikey={ApiKey}";
    }

    public int TileSize => 256;
}
