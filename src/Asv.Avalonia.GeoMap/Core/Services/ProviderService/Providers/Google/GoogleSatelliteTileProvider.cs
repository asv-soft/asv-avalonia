using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class GoogleSatelliteTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleSatelliteTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "GoogleSatellite";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.GoogleSatelliteTileProvider_Info_Name,
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

        return $"https://mt{server}.google.com/maps/vt/lyrs=s&hl=en&x={key.X}&y={key.Y}&z={key.Zoom}";
    }
}
