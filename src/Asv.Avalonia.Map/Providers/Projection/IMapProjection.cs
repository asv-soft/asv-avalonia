using Asv.Common;
using Avalonia;

namespace Asv.Avalonia.Map;

public interface IMapProjection
{
    Common.GeoPoint PixelsToWgs84(Point pixel, int zoom, int tileSize);
    Point Wgs84ToPixels(Common.GeoPoint wgs, int zoom, int tileSize);
}

public abstract class BaseMapProjection : IMapProjection
{
    public abstract Common.GeoPoint PixelsToWgs84(Point pixel, int zoom, int tileSize);
    public abstract Point Wgs84ToPixels(Common.GeoPoint wgs, int zoom, int tileSize);
}

public class WebMercatorProjection : BaseMapProjection
{
    public static IMapProjection Instance { get; } = new WebMercatorProjection();

    private WebMercatorProjection() { }

    public override Common.GeoPoint PixelsToWgs84(Point pixel, int zoom, int tileSize)
    {
        var globalPx = pixel.X;
        var globalPy = pixel.Y;
        var mapSize = tileSize * (1 << zoom); // 256 * 2^Zoom

        if (globalPx < 0)
            globalPx += mapSize;
        if (globalPx >= mapSize)
            globalPx -= mapSize;

        if (globalPy < 0)
            globalPy += mapSize;
        if (globalPy >= mapSize)
            globalPy -= mapSize;

        var lon = globalPx / mapSize * 360.0 - 180.0;
        var latRad = Math.PI * (1 - 2 * globalPy / mapSize);
        var lat = Math.Atan(Math.Sinh(latRad)) * 180.0 / Math.PI;

        return new Common.GeoPoint(lat, lon, 0);
    }

    public override Point Wgs84ToPixels(Common.GeoPoint wgs, int zoom, int tileSize)
    {
        var mapSize = tileSize * Math.Pow(2, zoom);

        var x = (wgs.Longitude + 180.0) / 360.0 * mapSize;
        var sinLat = Math.Sin(wgs.Latitude * Math.PI / 180.0);
        var y = (0.5 - Math.Log((1 + sinLat) / (1 - sinLat)) / (4 * Math.PI)) * mapSize;

        return new Point(x, y);
    }
}
