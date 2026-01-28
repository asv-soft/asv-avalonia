namespace Asv.Avalonia.GeoMap;

public class TileKey : IEquatable<TileKey>
{
    public TileKey(int x, int y, ushort zoom, ITileProvider provider)
    {
        X = x;
        var max = 1 << zoom;
        if (X < 0)
        {
            X += max;
        }

        if (X > max)
        {
            X -= max;
        }

        Y = y;
        if (Y < 0)
        {
            Y += max;
        }

        if (Y > max)
        {
            Y -= max;
        }

        Zoom = zoom;
        Provider = provider;
    }

    public int X { get; }
    public int Y { get; }
    public ushort Zoom { get; }
    public ITileProvider Provider { get; }

    public bool Equals(TileKey? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        return X == other.X
            && Y == other.Y
            && Zoom == other.Zoom
            && string.Equals(
                Provider.Info.Id,
                other.Provider.Info.Id,
                StringComparison.InvariantCultureIgnoreCase
            );
    }

    public override bool Equals(object? obj)
    {
        return obj is TileKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(X);
        hashCode.Add(Y);
        hashCode.Add(Zoom);
        hashCode.Add(Provider.Info.Id, StringComparer.InvariantCultureIgnoreCase);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(TileKey left, TileKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TileKey left, TileKey right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{Provider.Info.Id}[x:{X}, y:{Y}, z:{Zoom}]";
    }
}
