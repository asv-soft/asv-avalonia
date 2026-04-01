namespace Asv.Avalonia.GeoMap;

public class GoogleMapTileProvider : ITileProvider
{
    public const string Id = "GoogleMap";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.GoogleMapTileProvider_Info_Name,
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

        return $"https://mt{server}.google.com/maps/vt/lyrs=m&hl=en&x={key.X}&y={key.Y}&z={key.Zoom}";
    }

    public int TileSize => 256;
}
