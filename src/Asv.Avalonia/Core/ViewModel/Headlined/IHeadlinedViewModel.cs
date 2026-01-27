using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia;

public interface IHeadlinedViewModel : IRoutable
{
    /// <summary>
    /// Gets or sets the icon associated with the view model.
    /// </summary>
    MaterialIconKind? Icon { get; set; }

    /// <summary>
    /// Gets or sets the brush for the icon.
    /// </summary>
    AsvColorKind IconColor { get; set; }

    /// <summary>
    /// Gets or sets the header (title) of the view model.
    /// </summary>
    string? Header { get; set; }

    /// <summary>
    /// Gets or sets the description of the view model.
    /// </summary>
    string? Description { get; set; }

    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the order of the view model.
    /// </summary>
    int Order { get; set; }
}

public class HeadlinedComparer : IComparer<IHeadlinedViewModel>
{
    public static IComparer<IHeadlinedViewModel> Instance { get; } = new HeadlinedComparer();

    public int Compare(IHeadlinedViewModel? x, IHeadlinedViewModel? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is null)
        {
            return -1;
        }

        return x.Order.CompareTo(y.Order);
    }
}
