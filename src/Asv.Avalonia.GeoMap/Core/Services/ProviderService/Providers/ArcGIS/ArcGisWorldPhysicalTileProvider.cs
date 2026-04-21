namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldPhysicalTileProvider : ITileProvider
{
    public const string Id = "ArcGisWorldPhysical";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldPhysicalTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
        MinZoom = 1,
        MaxZoom = 8,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }

    public int TileSize => 256;
}
