namespace Asv.Avalonia.Comparers;

public class LogItemDescendingComparer : IComparer<LogItemViewModel>
{
    public int Compare(LogItemViewModel? x, LogItemViewModel? y)
    {
        if (y != null && x != null)
        {
            return y.Index.CompareTo(x.Index);
        }

        return -1;
    }
}
