namespace Asv.Avalonia.GeoMap;

public class BingHybridTileProvider : ITileProvider
{
    public const string Id = "BingTile";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.BingHybridTileProvider_Info_Name,
        Group = TileProviderGroup.Bing,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        var quadKey = GetQuadKey(key);
        var server = (key.X + key.Y) % 4;
        return $"https://ecn.t{server}.tiles.virtualearth.net/tiles/h{quadKey}.jpeg?g=4810&mkt=en-us&n=z";
    }

    private static string GetQuadKey(TileKey position)
    {
        var quadKey = new char[position.Zoom];

        for (var i = position.Zoom; i > 0; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);
            if ((position.X & mask) != 0)
            {
                digit++;
            }

            if ((position.Y & mask) != 0)
            {
                digit += (char)2;
            }

            quadKey[position.Zoom - i] = digit;
        }

        return new string(quadKey);
    }

    public int TileSize => 256;
}
