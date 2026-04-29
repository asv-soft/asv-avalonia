using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class WikiMapiaTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<WikiMapiaTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "WikiMapia";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.WikiMapiaTileProvider_Info_Name,
        Group = TileProviderGroup.Other,
        MinZoom = 1,
        MaxZoom = 18,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        var server = (key.X % 4) + ((key.Y % 4) * 4);
        if (server < 0)
        {
            server += 16;
        }

        return $"https://i{server}.wikimapia.org/?x={key.X}&y={key.Y}&zoom={key.Zoom}";
    }
}
