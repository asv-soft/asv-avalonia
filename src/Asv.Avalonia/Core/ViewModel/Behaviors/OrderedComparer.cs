using Asv.Modeling;

namespace Asv.Avalonia;

public class OrderedComparer : IComparer<ISupportOrder>
{
    public static readonly OrderedComparer Instance = new();
    
    public int Compare(ISupportOrder? x, ISupportOrder? y)
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