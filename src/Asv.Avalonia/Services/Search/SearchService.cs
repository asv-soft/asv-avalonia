using System.Composition;

namespace Asv.Avalonia;

[Export(typeof(ISearchService))]
[Shared]
public class SearchService : ISearchService
{
    [ImportingConstructor]
    public SearchService() { }

    public bool Match(string? text, string? query, out Selection match)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            match = Selection.Empty;
            return true;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            match = Selection.Empty;
            return false;
        }

        var index = text.Normalize().IndexOf(query.Normalize(), StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            match = Selection.Empty;
            return false;
        }

        match = new Selection(index, query.Length);
        return true;
    }
}
