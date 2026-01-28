namespace Asv.Avalonia;

public sealed class CommandInfoComparer : IComparer<ICommandInfo>
{
    public static readonly CommandInfoComparer Instance = new();

    private CommandInfoComparer() { }

    public int Compare(ICommandInfo? x, ICommandInfo? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return 1;
        }

        if (y is null)
        {
            return -1;
        }

        return string.CompareOrdinal(x.Id, y.Id);
    }
}
