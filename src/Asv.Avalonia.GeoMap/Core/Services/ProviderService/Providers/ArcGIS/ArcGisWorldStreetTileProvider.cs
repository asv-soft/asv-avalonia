using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldStreetTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ArcGisWorldStreetTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ArcGisWorldStreet";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldStreetTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
        MinZoom = 1,
        MaxZoom = 17,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }
}
