using System.Collections.Immutable;
using System.Globalization;
using Asv.Cfg;
using R3;
using ArgumentException = System.ArgumentException;

namespace Asv.Avalonia;

/// <summary>
/// Represents the default <see cref="ILanguageInfo"/> implementation.
/// </summary>
public class LanguageInfo : ILanguageInfo
{
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

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <summary>
    /// Gets the culture applied when the language is selected. The culture is resolved lazily on first access.
    /// </summary>
    public CultureInfo Culture => field ??= _getCulture();
}

public class LocalizationServiceConfig
{
    public string? CurrentLanguage { get; set; }
}

public class LocalizationService
    : ServiceWithConfigBase<LocalizationServiceConfig>,
        ILocalizationService
{
    private readonly ImmutableArray<LanguageInfo> _languages =
    [
        new("en", "English (EN)", () => CultureInfo.GetCultureInfo("en")),
        new("ru", "Русский (RU)", () => CultureInfo.GetCultureInfo("ru")),
    ];

    /// <inheritdoc />
    /// <remarks>
    /// This implementation requires assigned values to be <see cref="LanguageInfo"/> instances because
    /// the culture is stored on the concrete type.
    /// </remarks>
    public SynchronizedReactiveProperty<ILanguageInfo> CurrentLanguage { get; }

    /// <inheritdoc />
    public IEnumerable<ILanguageInfo> AvailableLanguages => _languages;

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
        _sub1 = CurrentLanguage.Subscribe(SetLanguage);
    }

    private void SetLanguage(ILanguageInfo lang)
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

        InternalSaveConfig(c => c.CurrentLanguage = lang.Id);
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
