using Avalonia.Styling;

namespace Asv.Avalonia;

public class ThemeItem(string id, string name, ThemeVariant theme)
    : IThemeInfo,
        IEqualityComparer<ThemeItem>
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public ThemeVariant Theme { get; } = theme;

    public bool Equals(ThemeItem? x, ThemeItem? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return string.Equals(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(ThemeItem obj)
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Id);
    }
}
