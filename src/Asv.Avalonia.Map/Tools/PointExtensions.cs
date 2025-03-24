using Avalonia;

namespace Asv.Avalonia.Map;

public static class PointExtensions
{
    /// <summary>
    /// Calculates the length of the vector.
    /// </summary>
    /// <remarks>The distance from the origin to the point.</remarks>
    /// <param name="point">.</param>
    /// <returns>..</returns>
    public static double Length(this Point point)
    {
        return Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
    }

    /// <summary>
    /// Normalizes the vector.
    /// </summary>
    /// <remarks>Scales vector to the unit length.</remarks>
    /// <param name="point">.</param>
    /// <returns>..</returns>
    public static Point Normalize(this Point point)
    {
        var length = point.Length();
        if (length < double.Epsilon) // Check for zero length to avoid division by zero
        {
            return new Point(0, 0);
        }

        return new Point(point.X / length, point.Y / length);
    }

    /// <summary>
    /// Calculates the squared length.
    /// </summary>
    /// <remarks>
    /// Is Optional!
    /// Is faster than Length if the square root is not needed.
    /// </remarks>
    /// <param name="point">The point representing the vector.</param>
    /// <returns>..</returns>
    // Optionally: calculate the squared length (faster than Length if the square root is not needed)
    public static double LengthSquared(this Point point)
    {
        return (point.X * point.X) + (point.Y * point.Y);
    }
}
