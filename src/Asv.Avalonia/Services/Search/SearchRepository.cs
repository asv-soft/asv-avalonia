namespace Asv.Avalonia;

public class SearchRepository
{
    private readonly List<SearchableItem> _items = [];

    public void AddItem(SearchableItem item) => _items.Add(item);

    public IEnumerable<SearchableItem> GetItems() => _items;

    public void Clear() => _items.Clear();
}

public class SearchableItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required SearchableItemType Type { get; init; }
}

public enum SearchableItemType
{
    Command,
    Setting,
    View,
    Other,
}
