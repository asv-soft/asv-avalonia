using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class GoogleHybridTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleHybridTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "GoogleHybrid";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.GoogleHybridTileProvider_Info_Name,
        Group = TileProviderGroup.Google,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        var server = (key.X + (2 * key.Y)) % 4;
        if (server < 0)
        {
            server += 4;
        }

        return $"https://mt{server}.google.com/maps/vt/lyrs=y&hl=en&x={key.X}&y={key.Y}&z={key.Zoom}";
    }
}
