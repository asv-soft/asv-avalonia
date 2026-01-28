namespace Asv.Avalonia;

public interface ISearchService
{
    bool Match(string? text, string? query, out Selection selection);
}

public readonly struct Selection(int start, int length)
{
    public static Selection Empty => new(0, 0);
    public int Start => start;
    public int Length => length;

    public int Stop => start + length;
}
