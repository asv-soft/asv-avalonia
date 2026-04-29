using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class OpenStreetMapTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<OpenStreetMapTileProvider> logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider)
{
    public const string Id = "OpenStreetMap";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.OpenStreetMapTileProvider_Info_Name,
        Group = TileProviderGroup.OpenStreetMap,
    };

    private static readonly string[] ServerLetters = ["a", "b", "c"];

    public override TileProviderInfo Info => StaticInfo;
    public override IMapProjection Projection => WebMercatorProjection.Instance;

    protected override string GetTileUrl(TileKey key)
    {
        var serverIndex = (key.X + key.Y) % ServerLetters.Length;
        if (serverIndex < 0)
        {
            serverIndex += ServerLetters.Length;
        }

        var server = ServerLetters[serverIndex];
        return $"https://{server}.tile.openstreetmap.org/{key.Zoom}/{key.X}/{key.Y}.png";
    }
}
