namespace Asv.Avalonia.GeoMap;

public class ThunderforestAtlasTileProvider : IProtectedTileProvider
{
    public const string Id = "ThunderforestAtlas";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ThunderforestAtlasTileProvider_Info_Name,
        Group = TileProviderGroup.Thunderforest,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    public string? GetTileUrl(TileKey key)
    {
        return $"https://api.thunderforest.com/atlas/{key.Zoom}/{key.X}/{key.Y}.png?apikey={ApiKey}";
    }

    public int TileSize => 256;
}
