namespace Asv.Avalonia.GeoMap;

public class YandexSatelliteTileProvider : ITileProvider
{
    public const string Id = "YandexSatellite";

    public static readonly TileProviderInfo StaticInfo = new()
    {
        Id = Id,
        NameCallback = () => RS.YandexSatelliteTileProvider_Info_Name,
        Group = TileProviderGroup.Yandex,
    };

    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        return $"https://core-sat.maps.yandex.net/tiles?l=sat&x={key.X}&y={key.Y}&z={key.Zoom}";
    }

    public int TileSize => 256;
}
