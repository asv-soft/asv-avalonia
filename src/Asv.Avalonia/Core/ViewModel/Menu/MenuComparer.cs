namespace Asv.Avalonia;

public class MenuComparer : IComparer<IMenuItem>
{
    public static IComparer<IMenuItem> Instance { get; } = new MenuComparer();

    private MenuComparer() { }

    public int Compare(IMenuItem? x, IMenuItem? y)
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

        var orderComparison = x.Order.CompareTo(y.Order);
        if (orderComparison != 0)
        {
            return orderComparison;
        }

        return x.Id.CompareTo(y.Id);
    }
}
