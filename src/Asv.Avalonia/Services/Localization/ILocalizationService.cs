using R3;

namespace Asv.Avalonia;

public interface ILocalizationService
{
    /// <summary>
    /// Allows you to select or get the current application language.
    /// </summary>
    ReactiveProperty<ILanguageInfo> CurrentLanguage { get; }

    /// <summary>
    /// Returns the list of available languages.
    /// </summary>
    IEnumerable<ILanguageInfo> AvailableLanguages { get; }
}
