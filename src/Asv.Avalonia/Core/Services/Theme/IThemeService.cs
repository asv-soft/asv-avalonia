using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents a theme item.
/// </summary>
public interface IThemeInfo
{
    string Id { get; }
    string Name { get; }
}

public interface IThemeService
{
    IEnumerable<IThemeInfo> Themes { get; }
    SynchronizedReactiveProperty<IThemeInfo> CurrentTheme { get; }
    SynchronizedReactiveProperty<bool> IsCompact { get; }
}
