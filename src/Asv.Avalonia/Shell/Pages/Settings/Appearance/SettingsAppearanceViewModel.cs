using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class SettingsAppearanceViewModelConfig : TreeSubpageConfig { }

[ExportSettings(PageId)]
public class SettingsAppearanceViewModel : SettingsSubPage<SettingsAppearanceViewModelConfig>
{
    public const string PageId = "appearance";

    #region DesignTime

    public SettingsAppearanceViewModel()
        : this(
            DesignTime.ThemeService,
            DesignTime.LocalizationService,
            NullDialogService.Instance,
            DesignTime.Configuration,
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
        IConfiguration cfg,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cfg, loggerFactory)
    {
        Theme = new ThemeProperty(themeService, loggerFactory) { Parent = this }.DisposeItWith(
            Disposable
        );
        Language = new LanguageProperty(localizationService, dialog, loggerFactory)
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
