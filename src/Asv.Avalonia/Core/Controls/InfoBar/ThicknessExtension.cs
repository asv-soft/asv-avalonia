using Avalonia;

namespace Asv.Avalonia;

public static class ThicknessMixin
{
    /// <summary>
    /// Retrieves the total vertical thickness (top + bottom).
    /// </summary>
    /// /// <param name="t">.</param>
    /// <returns>..</returns>
    public static double Vertical(this Thickness t)
    {
        return t.Top + t.Bottom;
    }

    /// <summary>
    /// Retrieves the total horizontal thickness (left + right).
    /// </summary>
    /// <param name="t">.</param>
    /// <returns>..</returns>
    public static double Horizontal(this Thickness t)
    {
        return t.Left + t.Right;
    }
}
