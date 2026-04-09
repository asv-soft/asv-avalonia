using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class BingRoadTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<BingRoadTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "BingRoad";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.BingRoadTileProvider_Info_Name,
        Group = TileProviderGroup.Bing,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        var quadKey = GetQuadKey(key);
        var server = (key.X + key.Y) % 4;
        return $"https://ecn.t{server}.tiles.virtualearth.net/tiles/r{quadKey}.jpeg?g=4810&mkt=en-us&n=z";
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
}
