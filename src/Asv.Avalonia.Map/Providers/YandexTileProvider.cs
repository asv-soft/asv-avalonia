namespace Asv.Avalonia.Map;

public class YandexTileProvider : ITileProvider
{
    public string? GetTileUrl(TileKey key)
    {
        return $"https://core-renderer-tiles.maps.yandex.net/tiles?l=map&x={key.X}&y={key.Y}&z={key.Zoom}";
    }

    public int TileSize => 256;
}
