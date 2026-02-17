using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class SettingAppearanceTreePageMenu : TreePage
{
    public SettingAppearanceTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsAppearanceViewModel.PageId,
            RS.SettingsAppearanceViewModel_Name,
            MaterialIconKind.ThemeLightDark,
            SettingsAppearanceViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
