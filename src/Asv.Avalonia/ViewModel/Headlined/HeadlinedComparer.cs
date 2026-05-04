namespace Asv.Avalonia;

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