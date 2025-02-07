using Asv.Common;

namespace Asv.Avalonia.Map;

public readonly struct TilePosition(int x, int y, int zoom) : IEquatable<TilePosition>
{
    public int X => x;
    public int Y => y;
    public int Zoom => zoom;

    public bool Equals(TilePosition other) => X == other.X && Y == other.Y && Zoom == other.Zoom;

    public override bool Equals(object? obj) => obj is TilePosition other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Zoom);

    public static bool operator ==(TilePosition left, TilePosition right) => left.Equals(right);

    public static bool operator !=(TilePosition left, TilePosition right) => !left.Equals(right);

    public static TilePosition FromGeoPoint(GeoPoint point, int tileSize, int zoom)
    {
        var sinLatitude = Math.Sin(point.Latitude * Math.PI / 180);
        var pixelX = ((point.Longitude + 180.0) / 360.0) * tileSize * (1 << zoom);
        var pixelY =
            (0.5 - (Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI)))
            * tileSize
            * (1 << zoom);

        var tileX = (int)(pixelX / tileSize);
        var tileY = (int)(pixelY / tileSize);

        return new TilePosition(tileX, tileY, zoom);
    }

    /// <summary>
    /// Преобразует координаты тайла обратно в географические координаты.
    /// </summary>
    public GeoPoint ToGeoPoint()
    {
        var n = Math.PI - ((2.0 * Math.PI * Y) / Math.Pow(2.0, Zoom));
        var lat = (180.0 / Math.PI) * Math.Atan(Math.Sinh(n));
        var lon = (X / Math.Pow(2.0, Zoom) * 360.0) - 180.0;

        return new GeoPoint(lat, lon, 0);
    }

    public string GetQuadKey()
    {
        var quadKey = new char[zoom];
        for (var i = zoom; i > 0; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);
            if ((x & mask) != 0)
            {
                digit++;
            }

            if ((y & mask) != 0)
            {
                digit += (char)2;
            }

            quadKey[zoom - i] = digit;
        }

        return new string(quadKey);
    }

    public override string ToString()
    {
        return $"{nameof(TilePosition)}(x:{X}, y:{Y}, zoom:{Zoom}, q: {GetQuadKey()})";
    }
}
