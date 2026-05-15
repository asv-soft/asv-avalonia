using System.Collections.Immutable;
using System.Globalization;
using R3;

namespace Asv.Avalonia;

public class NullLocalizationService : ILocalizationService, IDisposable
{
    public static ILocalizationService Instance { get; } = new NullLocalizationService();

    private readonly ImmutableArray<ILanguageInfo> _languages =
    [
        new LanguageInfo("en", "English (EN)", () => CultureInfo.GetCultureInfo("en")),
        new LanguageInfo("ru", "Русский (RU)", () => CultureInfo.GetCultureInfo("ru")),
    ];

    public SynchronizedReactiveProperty<ILanguageInfo> CurrentLanguage { get; } = new();
    public IEnumerable<ILanguageInfo> AvailableLanguages => _languages;

    private NullLocalizationService() { }

    public void Dispose()
    {
        CurrentLanguage.Dispose();
    }
}
