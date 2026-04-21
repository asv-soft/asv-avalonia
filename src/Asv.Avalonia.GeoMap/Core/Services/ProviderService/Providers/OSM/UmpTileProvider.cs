namespace Asv.Avalonia.GeoMap;

public class UmpTileProvider : ITileProvider
{
    public const string Id = "UMP";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.UmpTileProvider_Info_Name,
        Group = TileProviderGroup.OpenStreetMap,
        MinZoom = 1,
        MaxZoom = 18,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        return $"http://tiles.ump.waw.pl/ump_tiles/{key.Zoom}/{key.X}/{key.Y}.png";
    }

    public int TileSize => 256;
}
