namespace Asv.Avalonia.Plugins;

public class SearchQuery
{
    public static readonly SearchQuery Empty = new()
    {
        Name = null,
        IncludePrerelease = false,
        Skip = 0,
        Take = 20,
    };

    public string? Name { get; set; }
    public bool IncludePrerelease { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
    public HashSet<string> Sources { get; } = [];
}
