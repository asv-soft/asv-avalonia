using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ThunderforestCycleTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ThunderforestCycleTileProvider> logger,
    TimeProvider timeProvider
) : ProtectedHttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ThunderforestCycle";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ThunderforestCycleTileProvider_Info_Name,
        Group = TileProviderGroup.Thunderforest,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://api.thunderforest.com/cycle/{key.Zoom}/{key.X}/{key.Y}.png?apikey={ApiKey}";
    }
}
