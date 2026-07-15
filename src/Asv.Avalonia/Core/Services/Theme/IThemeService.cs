using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents a theme item.
/// </summary>
public interface IThemeInfo
{
    /// <summary>
    /// Gets the unique theme identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the localized display name.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// Manages the application's theme and control density.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets all available themes.
    /// </summary>
    IEnumerable<IThemeInfo> Themes { get; }

    /// <summary>
    /// Gets the active theme. Assigning a <see cref="ThemeItem"/> applies its variant and persists its
    /// identifier when it changes. Other <see cref="IThemeInfo"/> implementations are rejected.
    /// </summary>
    SynchronizedReactiveProperty<IThemeInfo> CurrentTheme { get; }

    /// <summary>
    /// Gets a reactive property that indicates whether compact density is enabled. Assigning a value
    /// applies the density and persists it.
    /// </summary>
    SynchronizedReactiveProperty<bool> IsCompact { get; }
}
