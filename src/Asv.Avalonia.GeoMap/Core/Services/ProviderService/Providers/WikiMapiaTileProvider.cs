namespace Asv.Avalonia.GeoMap;

public class WikiMapiaTileProvider : ITileProvider
{
    public const string Id = "WikiMapia";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.WikiMapiaTileProvider_Info_Name,
        Group = TileProviderGroup.Other,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        var server = (key.X % 4) + ((key.Y % 4) * 4);
        if (server < 0)
        {
            server += 16;
        }

        return $"https://i{server}.wikimapia.org/?x={key.X}&y={key.Y}&zoom={key.Zoom}";
    }

    public int TileSize => 256;
}
