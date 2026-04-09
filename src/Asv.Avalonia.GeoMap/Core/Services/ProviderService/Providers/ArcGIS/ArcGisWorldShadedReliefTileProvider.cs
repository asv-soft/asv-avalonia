using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldShadedReliefTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ArcGisWorldShadedReliefTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ArcGisWorldShadedRelief";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldShadedReliefTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
        MinZoom = 1,
        MaxZoom = 13,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/ArcGIS/rest/services/World_Shaded_Relief/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }
}
