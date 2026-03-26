using Microsoft.Extensions.Options;

namespace Asv.Avalonia.GeoMap;

public class BingTileProviderOptions
{
    public const string ConfigurationSection = "Map:TileProviders:Bing";
    public bool UseHighRes { get; set; } = false;
}

public class BingTileProvider : IProtectedTileProvider
{
    public const string Id = "BingTile";
    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.BingTileProvider_Info_Name,
        Group = TileProviderGroup.Bing,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public string? ApiKey { get; set; }

    private readonly bool _useHighRes;

    public BingTileProvider(IOptions<BingTileProviderOptions> apiKey)
    {
        _useHighRes = apiKey.Value.UseHighRes;
    }

    public string? GetTileUrl(TileKey position)
    {
        var quadKey = GetQuadKey(position);
        return $"https://t0.ssl.ak.dynamic.tiles.virtualearth.net/comp/CompositionHandler/{quadKey}?mkt=en-US&it=A,G,L&dpi={(_useHighRes ? "d1" : "d0")}&key={ApiKey}";
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

    public int TileSize => _useHighRes ? 512 : 256;
}
