using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldPhysicalTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ArcGisWorldPhysicalTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ArcGisWorldPhysical";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldPhysicalTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
        MinZoom = 1,
        MaxZoom = 8,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }
}
