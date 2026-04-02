namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldShadedReliefTileProvider : ITileProvider
{
    public const string Id = "ArcGisWorldShadedRelief";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldShadedReliefTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/ArcGIS/rest/services/World_Shaded_Relief/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }

    public int TileSize => 256;
}
