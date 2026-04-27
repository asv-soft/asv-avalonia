using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class YandexSatelliteTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<YandexSatelliteTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "YandexSatellite";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.YandexSatelliteTileProvider_Info_Name,
        Group = TileProviderGroup.Yandex,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://core-sat.maps.yandex.net/tiles?l=sat&x={key.X}&y={key.Y}&z={key.Zoom}";
    }
}
