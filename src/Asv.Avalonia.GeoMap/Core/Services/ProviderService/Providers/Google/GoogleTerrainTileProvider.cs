using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class GoogleTerrainTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleTerrainTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "GoogleTerrain";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.GoogleTerrainTileProvider_Info_Name,
        Group = TileProviderGroup.Google,
        MinZoom = 1,
        MaxZoom = 15,
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

        return $"https://mt{server}.google.com/maps/vt/lyrs=p&hl=en&x={key.X}&y={key.Y}&z={key.Zoom}";
    }
}
