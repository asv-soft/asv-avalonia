using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ArcGisWorldTerrainTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ArcGisWorldTerrainTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ArcGisWorldTerrain";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ArcGisWorldTerrainTileProvider_Info_Name,
        Group = TileProviderGroup.ArcGis,
        MinZoom = 1,
        MaxZoom = 9,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://server.arcgisonline.com/ArcGIS/rest/services/World_Terrain_Base/MapServer/tile/{key.Zoom}/{key.Y}/{key.X}";
    }
}
