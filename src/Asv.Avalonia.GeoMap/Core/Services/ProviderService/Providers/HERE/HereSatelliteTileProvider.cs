using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class HereSatelliteTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<HereSatelliteTileProvider> logger,
    TimeProvider timeProvider
) : ProtectedHttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "HereSatellite";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.HereSatelliteTileProvider_Info_Name,
        Group = TileProviderGroup.Here,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://maps.hereapi.com/v3/satellite/mc/{key.Zoom}/{key.X}/{key.Y}/png?apiKey={ApiKey}";
    }
}
