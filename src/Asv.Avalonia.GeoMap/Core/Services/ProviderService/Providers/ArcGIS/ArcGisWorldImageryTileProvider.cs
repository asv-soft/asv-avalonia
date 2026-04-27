using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldImageryTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ArcGisWorldImageryTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ArcGisWorldImagery";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldImageryTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
        MinZoom = 1,
        MaxZoom = 17,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }
}
