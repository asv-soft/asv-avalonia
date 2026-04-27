using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ThunderforestTransportDarkTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ThunderforestTransportDarkTileProvider> logger,
    TimeProvider timeProvider
) : ProtectedHttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ThunderforestTransportDark";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ThunderforestTransportDarkTileProvider_Info_Name,
        Group = TileProviderGroup.Thunderforest,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://api.thunderforest.com/transport-dark/{key.Zoom}/{key.X}/{key.Y}.png?apikey={ApiKey}";
    }
}
