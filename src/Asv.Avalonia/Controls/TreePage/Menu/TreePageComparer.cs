namespace Asv.Avalonia;

public class TreePageComparer : IComparer<ITreePage>
{
    public static IComparer<ITreePage> Instance { get; } = new TreePageComparer();

    private TreePageComparer() { }

    public int Compare(ITreePage? x, ITreePage? y)
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