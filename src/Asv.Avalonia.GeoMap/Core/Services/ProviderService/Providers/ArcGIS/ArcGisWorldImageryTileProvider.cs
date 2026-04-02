namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldImageryTileProvider : ITileProvider
{
    public const string Id = "ArcGisWorldImagery";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldImageryTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }

    public int TileSize => 256;
}
