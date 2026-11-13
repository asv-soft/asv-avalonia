using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia;

public class FitFillPanel : Panel
{
    static FitFillPanel()
    {
        AffectsParentMeasure<FitFillPanel>(MinHeightProperty, MarginProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = GetVisibleChildren();
        if (children.Count == 0)
        {
            return default;
        }

        var fill = children[^1];
        var reserved = fill.MinHeight + fill.Margin.Top + fill.Margin.Bottom;
        var width = 0.0;
        var height = 0.0;
        for (var i = 0; i < children.Count - 1; i++)
        {
            var child = children[i];
            child.Measure(
                new Size(availableSize.Width, Math.Max(0, availableSize.Height - height - reserved))
            );
            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;
        }

        fill.Measure(new Size(availableSize.Width, Math.Max(0, availableSize.Height - height)));
        width = Math.Max(width, fill.DesiredSize.Width);
        return new Size(width, height + fill.DesiredSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = GetVisibleChildren();
        if (children.Count == 0)
        {
            return finalSize;
        }

        var y = 0.0;
        for (var i = 0; i < children.Count - 1; i++)
        {
            var child = children[i];
            child.Arrange(new Rect(0, y, finalSize.Width, child.DesiredSize.Height));
            y += child.DesiredSize.Height;
        }

        children[^1].Arrange(new Rect(0, y, finalSize.Width, Math.Max(0, finalSize.Height - y)));
        return finalSize;
    }

    private List<Control> GetVisibleChildren() => Children.Where(c => c.IsVisible).ToList();
}
