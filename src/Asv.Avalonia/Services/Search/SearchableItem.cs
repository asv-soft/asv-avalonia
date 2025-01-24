namespace Asv.Avalonia;

public class SearchableItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required SearchableItemType Type { get; init; }
}

[AttributeUsage(AttributeTargets.Class)]
public class SearchableAttribute(
    string id,
    string name,
    string? description,
    SearchableItemType type
) : Attribute
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string? Description { get; set; } = description;
    public SearchableItemType Type { get; set; } = type;
}

public enum SearchableItemType
{
    Command,
    Setting,
    View,
    Other,
}
