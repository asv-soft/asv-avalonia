namespace Asv.Avalonia;

public class TreePageComparer : IComparer<ITreePageMenuItem>
{
    public static IComparer<ITreePageMenuItem> Instance { get; } = new TreePageComparer();

    private TreePageComparer() { }

    public int Compare(ITreePageMenuItem? x, ITreePageMenuItem? y)
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

        return string.CompareOrdinal(x.Id.TypeId, y.Id.TypeId);
    }
}
