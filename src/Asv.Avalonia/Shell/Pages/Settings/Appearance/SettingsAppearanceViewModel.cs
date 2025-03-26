using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

[ExportSettings(PageId)]
public class SettingsAppearanceViewModel : SettingsSubPage
{
    public const string PageId = "appearance";

    #region DesignTime

    public SettingsAppearanceViewModel()
        : this(DesignTime.ThemeService, DesignTime.LocalizationService, null!)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    #endregion

    [ImportingConstructor]
    public SettingsAppearanceViewModel(
        IThemeService themeService,
        ILocalizationService localizationService,
        IDialogService dialogService
    )
        : base(PageId)
    {
        Theme = new ThemeProperty(themeService) { Parent = this }.DisposeItWith(Disposable);
        Language = new LanguageProperty(localizationService, dialogService)
        {
            Parent = this,
        }.DisposeItWith(Disposable);
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
