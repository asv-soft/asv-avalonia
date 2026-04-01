namespace Asv.Avalonia.GeoMap;

public class GoogleTerrainTileProvider : ITileProvider
{
    public const string Id = "GoogleTerrain";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.GoogleTerrainTileProvider_Info_Name,
        Group = TileProviderGroup.Google,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        var server = (key.X + (2 * key.Y)) % 4;
        if (server < 0)
        {
            server += 4;
        }

        return $"https://mt{server}.google.com/maps/vt/lyrs=p&hl=en&x={key.X}&y={key.Y}&z={key.Zoom}";
    }

    public int TileSize => 256;
}
