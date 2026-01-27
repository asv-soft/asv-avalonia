using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.GeoMap;

public class VerticalOffsetExtension : MarkupExtension
{
    private readonly VerticalOffsetEnum _offsetType;
    private readonly double _offset;

    public VerticalOffsetExtension(VerticalOffsetEnum offsetType)
    {
        _offsetType = offsetType;
    }

    public VerticalOffsetExtension(VerticalOffsetEnum offsetType, double offset)
    {
        _offsetType = offsetType;
        _offset = offset;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new VerticalOffset(_offsetType, _offset);
    }
}

public readonly struct VerticalOffset(VerticalOffsetEnum offsetType, double offset)
{
    public static readonly VerticalOffset Default = new(VerticalOffsetEnum.Center, 0);
    public VerticalOffsetEnum OffsetType => offsetType;
    public double Offset => offset;

    public double CalculateOffset(double boundsHeight)
    {
        return OffsetType switch
        {
            VerticalOffsetEnum.Top => Offset,
            VerticalOffsetEnum.Center => (boundsHeight / 2) - Offset,
            VerticalOffsetEnum.Bottom => boundsHeight - Offset,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}

/// <summary>
/// Represents the possible vertical offset positions for an
/// </summary>
public enum VerticalOffsetEnum
{
    /// <summary>
    /// Enum member representing the vertical offset position from the center of the target element.
    /// </summary>
    Center,

    /// <summary>
    /// Represents the vertical offset position of an element.
    /// </summary>
    Top,

    /// <summary>
    /// Represents the vertical alignment of an element relative to its parent or container.
    /// </summary>
    Bottom,
}
