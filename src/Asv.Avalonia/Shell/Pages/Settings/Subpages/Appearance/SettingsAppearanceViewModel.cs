using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportSettings(PageId)]
public class SettingsAppearanceViewModel : SettingsSubPage
{
    public const string PageId = "appearance";

    #region DesignTime

    public SettingsAppearanceViewModel()
        : this(
            DesignTime.ThemeService,
            DesignTime.LocalizationService,
            NullDialogService.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    #endregion

    [ImportingConstructor]
    public SettingsAppearanceViewModel(
        IThemeService themeService,
        ILocalizationService localizationService,
        IDialogService dialog,
        ILoggerFactory loggerFactory
    )
        : base(PageId, loggerFactory)
    {
        Theme = new ThemeProperty(themeService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Language = new LanguageProperty(localizationService, dialog, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public ThemeProperty Theme { get; }
    public LanguageProperty Language { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Theme;
        yield return Language;

        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }
    }

    public override IExportInfo Source => SystemModule.Instance;
}
