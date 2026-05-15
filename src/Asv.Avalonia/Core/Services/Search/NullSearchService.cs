namespace Asv.Avalonia;

public sealed class NullSearchService : ISearchService
{
    public static NullSearchService Instance => new();

    private NullSearchService() { }

    public bool Match(string? text, string? query, out Selection selection)
    {
        selection = Selection.Empty;
        return true;
    }
}
