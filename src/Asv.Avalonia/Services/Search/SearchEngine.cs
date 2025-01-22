using FuzzySharp;

namespace Asv.Avalonia;

public class SearchEngine(SearchRepository searchRepository)
{
    public IEnumerable<SearchableItem> Search(string query, SearchableItemType? typeFilter = null)
    {
        var items = searchRepository.GetItems();

        if (typeFilter.HasValue)
        {
            items = items.Where(i => i.Type == typeFilter.Value);
        }

        return items
            .Select(item => new { Item = item, Score = Fuzz.PartialRatio(query, item.Name) })
            .Where(x => x.Score > 50)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item);
    }
}
