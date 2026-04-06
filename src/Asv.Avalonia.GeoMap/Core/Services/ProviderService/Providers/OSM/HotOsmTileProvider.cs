namespace Asv.Avalonia.GeoMap;

public class HotOsmTileProvider : ITileProvider
{
    public const string Id = "HotOSM";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.HotOsmTileProvider_Info_Name,
        Group = TileProviderGroup.OpenStreetMap,
    };

    private static readonly string[] ServerLetters = ["a", "b", "c"];

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        var serverIndex = (key.X + key.Y) % ServerLetters.Length;
        if (serverIndex < 0)
        {
            serverIndex += ServerLetters.Length;
        }

        var server = ServerLetters[serverIndex];
        return $"https://{server}.tile.openstreetmap.fr/hot/{key.Zoom}/{key.X}/{key.Y}.png";
    }

    public int TileSize => 256;
}
