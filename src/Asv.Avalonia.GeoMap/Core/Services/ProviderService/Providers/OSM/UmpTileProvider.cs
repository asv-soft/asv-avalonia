using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class UmpTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<UmpTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "UMP";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.UmpTileProvider_Info_Name,
        Group = TileProviderGroup.OpenStreetMap,
        MinZoom = 1,
        MaxZoom = 18,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"http://tiles.ump.waw.pl/ump_tiles/{key.Zoom}/{key.X}/{key.Y}.png";
    }
}
