namespace Asv.Avalonia;

public interface ITreeViewModel : IViewModel
{
    NavigationId ParentId { get; }
    int Order { get; }
}

public class TreeNodeComparer<T> : IComparer<T>
    where T : ITreeViewModel
{
    public static IComparer<T> Instance { get; } = new TreeNodeComparer<T>();

    private TreeNodeComparer() { }

    public int Compare(T? x, T? y)
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
