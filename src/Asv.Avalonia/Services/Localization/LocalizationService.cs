using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using Asv.Cfg;
using R3;
using ArgumentException = System.ArgumentException;

namespace Asv.Avalonia;

public class LanguageInfo : ILanguageInfo
{
    private CultureInfo? _culture;
    private readonly Func<CultureInfo> _getCulture;

    public LanguageInfo(string id, string displayName, Func<CultureInfo> getCulture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(getCulture);

        Id = id;
        DisplayName = displayName;
        _getCulture = getCulture;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public CultureInfo Culture => _culture ??= _getCulture();
}

public class LocalizationServiceConfig
{
    public string? CurrentLanguage { get; set; }
}

[Export(typeof(ILocalizationService))]
[Shared]
public class LocalizationService
    : ServiceWithConfigBase<LocalizationServiceConfig>,
        ILocalizationService
{
    private readonly ImmutableArray<LanguageInfo> _languages =
    [
        new("en", "English (EN)", () => CultureInfo.GetCultureInfo("en")),
        new("ru", "Русский (RU)", () => CultureInfo.GetCultureInfo("ru")),
    ];

    public SynchronizedReactiveProperty<ILanguageInfo> CurrentLanguage { get; }
    public IEnumerable<ILanguageInfo> AvailableLanguages => _languages;

    [ImportingConstructor]
    public LocalizationService(IConfiguration cfgSvc)
        : base(cfgSvc)
    {
        var selectedLang = default(LanguageInfo);
        var langFromConfig = InternalGetConfig(_ => _.CurrentLanguage);
        if (!string.IsNullOrWhiteSpace(langFromConfig))
        {
            selectedLang = _languages.FirstOrDefault(_ => _.Id.Equals(langFromConfig));
        }

        selectedLang ??= _languages[0];
        CurrentLanguage = new SynchronizedReactiveProperty<ILanguageInfo>(selectedLang);
        _sub1 = CurrentLanguage.SubscribeAwait(SetLanguage);
    }

    private ValueTask SetLanguage(ILanguageInfo lang, CancellationToken cancelToken)
    {
        ArgumentNullException.ThrowIfNull(lang);

        if (lang is not LanguageInfo langInfo)
        {
            throw new ArgumentException("Invalid language info", nameof(lang));
        }

        var culture = langInfo.Culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        InternalSaveConfig(_ => _.CurrentLanguage = lang.Id);

        return ValueTask.CompletedTask;
    }

    #region Dispose

    private readonly IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            CurrentLanguage.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
