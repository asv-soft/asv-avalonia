using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class YandexMapTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<YandexMapTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "Yandex";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.YandexTileProvider_Info_Name,
        Group = TileProviderGroup.Yandex,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://core-renderer-tiles.maps.yandex.net/tiles?l=map&x={key.X}&y={key.Y}&z={key.Zoom}";
    }
}
