using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Map;

public class VerticalOffsetExtension : MarkupExtension
{
    private readonly MapOffsetYEnum _offsetType;
    private readonly double _offset;

    public VerticalOffsetExtension(MapOffsetYEnum offsetType)
    {
        _offsetType = offsetType;
    }

    public VerticalOffsetExtension(MapOffsetYEnum offsetType, double offset)
    {
        _offsetType = offsetType;
        _offset = offset;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new VerticalOffset(_offsetType, _offset);
    }
}

public readonly struct VerticalOffset(MapOffsetYEnum offsetType, double offset)
{
    public MapOffsetYEnum OffsetType => offsetType;
    public double Offset => offset;

    public double CalculateOffset(double boundsHeight)
    {
        return OffsetType switch
        {
            MapOffsetYEnum.Top => Offset,
            MapOffsetYEnum.Center => boundsHeight / 2 - Offset,
            MapOffsetYEnum.Bottom => boundsHeight - Offset,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}

/// <summary>
/// Represents the possible vertical offset positions for an
/// </summary>
public enum MapOffsetYEnum
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
