using Asv.Common;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.GeoMap;

public class HorizontalOffsetExtension(HorizontalOffsetEnum offsetType) : MarkupExtension
{
    public double Offset { get; set; } = 0.0;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new HorizontalOffset(offsetType, Offset);
    }
}

public readonly struct HorizontalOffset(HorizontalOffsetEnum offsetType, double offset)
{
    public static readonly HorizontalOffset Default = new(HorizontalOffsetEnum.Center, 0);

    public HorizontalOffsetEnum OffsetType => offsetType;
    public double Offset => offset;

    public double CalculateOffset(double boundsWidth)
    {
        return OffsetType switch
        {
            HorizontalOffsetEnum.Left => Offset,
            HorizontalOffsetEnum.Center => (boundsWidth / 2) - Offset,
            HorizontalOffsetEnum.Right => boundsWidth - Offset,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}

/// <summary>
/// Enumeration for X-axis offset values.
/// </summary>
public enum HorizontalOffsetEnum
{
    /// <summary>
    /// Enumeration for X-axis offset values.
    /// </summary>
    Center,

    /// <summary>
    /// Represents the offset value for the X-axis of an element to be positioned on the left side.
    /// </summary>
    Left,

    /// <summary>
    /// Enumeration for X-axis offset values.
    /// </summary>
    Right,
}
