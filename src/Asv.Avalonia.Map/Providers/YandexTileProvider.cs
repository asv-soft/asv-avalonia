namespace Asv.Avalonia.Map;

public class YandexTileProvider : ITileProvider
{
    public string? GetTileUrl(TilePosition position)
    {
        return $"https://core-renderer-tiles.maps.yandex.net/tiles?l=map&x={position.X}&y={position.Y}&z={position.Zoom}";
    }

    public int TileSize => 256;
}
