using Asv.Common;

namespace Asv.Avalonia.Map;

public readonly struct TilePosition : IEquatable<TilePosition>
{
    private readonly int _x;
    private readonly int _y;
    private readonly int _zoom;

    public TilePosition(int x, int y, int zoom)
    {
        _x = x;
        var max = 1 << zoom;
        if (_x < 0)
        {
            _x += max;
        }
        if (_x > max)
        {
            _x -= max;
        }
        _y = y;
        if (_y < 0)
        {
            _y += max;
        }
        if (_y > max)
        {
            _y -= max;
        }
        _zoom = zoom;
    }

    public int X => _x;
    public int Y => _y;
    public int Zoom => _zoom;

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

    

    public override string ToString()
    {
        return $"{nameof(TilePosition)}(x:{X}, y:{Y}, zoom:{Zoom})";
    }
}
