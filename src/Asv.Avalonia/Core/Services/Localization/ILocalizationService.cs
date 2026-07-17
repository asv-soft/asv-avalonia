using R3;

namespace Asv.Avalonia;

/// <summary>
/// Manages the application's current UI language and available language choices.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the active language selected from <see cref="AvailableLanguages"/>.
    /// </summary>
    SynchronizedReactiveProperty<ILanguageInfo> CurrentLanguage { get; }

    /// <summary>
    /// Gets all available languages.
    /// </summary>
    IEnumerable<ILanguageInfo> AvailableLanguages { get; }
}
