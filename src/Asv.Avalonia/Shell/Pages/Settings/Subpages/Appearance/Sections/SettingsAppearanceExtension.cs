using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class SettingsAppearanceExtension(
    IThemeService themeService,
    ILocalizationService localizationService,
    IDialogService dialog,
    ILoggerFactory loggerFactory
) : IExtensionFor<ISettingsAppearanceSubPage>
{
    public void Extend(ISettingsAppearanceSubPage context, CompositeDisposable contextDispose)
    {
        context.Sections.Add(
            new CommonAppearanceSettingsSectionViewModel(
                themeService,
                localizationService,
                dialog,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );
    }
}
