using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class HereHybridTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<HereHybridTileProvider> logger,
    TimeProvider timeProvider
) : ProtectedHttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "HereHybrid";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.HereHybridTileProvider_Info_Name,
        Group = TileProviderGroup.Here,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://maps.hereapi.com/v3/base/mc/{key.Zoom}/{key.X}/{key.Y}/png?style=satellite.day&apiKey={ApiKey}";
    }
}
