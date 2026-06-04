using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class CommonAppearanceSettingsSectionViewModel : ViewModel, ISettingsAppearanceSection
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
        Editor = new ExtendedPropertyEditorViewModel($"{PageId}.editor")
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Theme = new ThemeProperty(themeService);
        Language = new LanguageProperty(localizationService, dialog);

        Editor.ItemsSource.Add(Theme);
        Editor.ItemsSource.Add(Language);
    }

    public ExtendedPropertyEditorViewModel Editor { get; }
    public ThemeProperty Theme { get; }
    public LanguageProperty Language { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Editor;
    }
}
