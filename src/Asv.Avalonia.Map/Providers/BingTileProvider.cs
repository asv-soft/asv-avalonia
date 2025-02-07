namespace Asv.Avalonia.Map;

public class BingTileProvider : ITileProvider
{
    private readonly string _apiKey;
    private readonly bool _useHighRes;

    public BingTileProvider(string apiKey, bool useHighRes = false)
    {
        _apiKey = apiKey;
        _useHighRes = useHighRes;
    }

    public string? GetTileUrl(TilePosition position)
    {
        var quadKey = position.GetQuadKey();
        return $"https://t0.ssl.ak.dynamic.tiles.virtualearth.net/comp/CompositionHandler/{quadKey}?mkt=en-US&it=A,G,L&dpi={(_useHighRes ? "d1" : "d0")}&key={_apiKey}";
    }

    public int TileSize => _useHighRes ? 512 : 256;
}
