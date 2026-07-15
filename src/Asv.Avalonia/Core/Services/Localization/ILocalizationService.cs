using R3;

namespace Asv.Avalonia;

/// <summary>
/// Manages the application's current UI language and available language choices.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the active language. Assigning an item from <see cref="AvailableLanguages"/> applies its
    /// culture and persists the choice. Other <see cref="ILanguageInfo"/> implementations are rejected.
    /// </summary>
    SynchronizedReactiveProperty<ILanguageInfo> CurrentLanguage { get; }

    /// <summary>
    /// Gets all available languages.
    /// </summary>
    IEnumerable<ILanguageInfo> AvailableLanguages { get; }
}
