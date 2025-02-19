namespace Asv.Avalonia.Map;



public class BingTileProvider : ITileProvider
{
    public const string Id = "BingTile";
    private static readonly TileProviderInfo StaticInfo = new(Id, "Bing map");
    
    
    private readonly string _apiKey;
    private readonly bool _useHighRes;
    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public BingTileProvider(string apiKey, bool useHighRes = false)
    {
        _apiKey = apiKey;
        _useHighRes = useHighRes;
    }

    public string? GetTileUrl(TilePosition position)
    {
        var quadKey = GetQuadKey(position);
        return $"https://t0.ssl.ak.dynamic.tiles.virtualearth.net/comp/CompositionHandler/{quadKey}?mkt=en-US&it=A,G,L&dpi={(_useHighRes ? "d1" : "d0")}&key={_apiKey}";
    }

    private static string GetQuadKey(TilePosition position)
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

    public int TileSize => _useHighRes ? 512 : 256;
}
