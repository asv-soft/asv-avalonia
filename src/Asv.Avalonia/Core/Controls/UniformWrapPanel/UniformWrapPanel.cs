using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia;

public class UniformWrapPanel : Panel
{
    public static readonly StyledProperty<double> MinItemWidthProperty = AvaloniaProperty.Register<
        UniformWrapPanel,
        double
    >(nameof(MinItemWidth), 200);

    public static readonly StyledProperty<double> ItemSpacingProperty = AvaloniaProperty.Register<
        UniformWrapPanel,
        double
    >(nameof(ItemSpacing));

    public static readonly StyledProperty<double> LineSpacingProperty = AvaloniaProperty.Register<
        UniformWrapPanel,
        double
    >(nameof(LineSpacing));

    static UniformWrapPanel()
    {
        AffectsMeasure<UniformWrapPanel>(
            MinItemWidthProperty,
            ItemSpacingProperty,
            LineSpacingProperty
        );
    }

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = GetVisibleChildren();
        if (children.Count == 0)
        {
            return default;
        }

        var columns = GetColumnCount(availableSize.Width, children.Count);
        var itemWidth = GetItemWidth(availableSize.Width, columns);
        var height = 0.0;
        for (var start = 0; start < children.Count; start += columns)
        {
            var rowHeight = 0.0;
            for (var i = start; i < Math.Min(start + columns, children.Count); i++)
            {
                children[i].Measure(new Size(itemWidth, double.PositiveInfinity));
                rowHeight = Math.Max(rowHeight, children[i].DesiredSize.Height);
            }

            height += rowHeight;
        }

        var rows = (children.Count + columns - 1) / columns;
        height += LineSpacing * (rows - 1);
        return new Size((itemWidth * columns) + (ItemSpacing * (columns - 1)), height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = GetVisibleChildren();
        if (children.Count == 0)
        {
            return finalSize;
        }

        var columns = GetColumnCount(finalSize.Width, children.Count);
        var itemWidth = GetItemWidth(finalSize.Width, columns);
        var y = 0.0;
        for (var start = 0; start < children.Count; start += columns)
        {
            var end = Math.Min(start + columns, children.Count);
            var rowHeight = 0.0;
            for (var i = start; i < end; i++)
            {
                rowHeight = Math.Max(rowHeight, children[i].DesiredSize.Height);
            }

            for (var i = start; i < end; i++)
            {
                var x = (i - start) * (itemWidth + ItemSpacing);
                children[i].Arrange(new Rect(x, y, itemWidth, rowHeight));
            }

            y += rowHeight + LineSpacing;
        }

        return finalSize;
    }

    private List<Control> GetVisibleChildren() => Children.Where(c => c.IsVisible).ToList();

    private int GetColumnCount(double width, int count) =>
        double.IsInfinity(width)
            ? count
            : Math.Max(1, (int)((width + ItemSpacing) / (MinItemWidth + ItemSpacing)));

    private double GetItemWidth(double width, int columns) =>
        double.IsInfinity(width) ? MinItemWidth : (width - (ItemSpacing * (columns - 1))) / columns;
}
