using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class CommonAppearanceSettingsSectionViewModel
    : ViewModel,
        ISettingsAppearanceSection
{
    public const string PageId = "common";

    public CommonAppearanceSettingsSectionViewModel()
        : this(
            DesignTime.ThemeService,
            DesignTime.LocalizationService,
            NullDialogService.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public CommonAppearanceSettingsSectionViewModel(
        IThemeService themeService,
        ILocalizationService localizationService,
        IDialogService dialog,
        ILoggerFactory loggerFactory
    )
        : base(PageId)
    {
        Theme = new ThemeProperty(themeService)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Language = new LanguageProperty(localizationService, dialog)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public ThemeProperty Theme { get; }
    public LanguageProperty Language { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Theme;
        yield return Language;
    }
}
