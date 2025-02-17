namespace Asv.Avalonia.Map;

public class TileKey(int x, int y, ushort zoom, string provider) : IEquatable<TileKey>
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public ushort Zoom { get; } = zoom;
    public string Provider { get; } = provider;

    public bool Equals(TileKey other)
    {
        return X == other.X
            && Y == other.Y
            && Zoom == other.Zoom
            && string.Equals(Provider, other.Provider, StringComparison.InvariantCultureIgnoreCase);
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
        hashCode.Add(Provider, StringComparer.InvariantCultureIgnoreCase);
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
}
