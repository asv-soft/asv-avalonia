using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class ThunderforestAtlasTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<ThunderforestAtlasTileProvider> logger,
    TimeProvider timeProvider
) : ProtectedHttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "ThunderforestAtlas";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.ThunderforestAtlasTileProvider_Info_Name,
        Group = TileProviderGroup.Thunderforest,
    };

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        return $"https://api.thunderforest.com/atlas/{key.Zoom}/{key.X}/{key.Y}.png?apikey={ApiKey}";
    }
}
