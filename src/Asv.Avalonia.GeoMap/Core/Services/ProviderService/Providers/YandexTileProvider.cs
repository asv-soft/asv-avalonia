namespace Asv.Avalonia.GeoMap;

public class YandexTileProvider : ITileProvider
{
    public const string Id = "Yandex";
    public static readonly TileProviderInfo StaticInfo = new(Id, "Yandex");
    public TileProviderInfo Info => StaticInfo;
    public IMapProjection Projection => WebMercatorProjection.Instance;

    public string? GetTileUrl(TileKey key)
    {
        return $"https://core-renderer-tiles.maps.yandex.net/tiles?l=map&x={key.X}&y={key.Y}&z={key.Zoom}";
    }

    public int TileSize => 256;
}
