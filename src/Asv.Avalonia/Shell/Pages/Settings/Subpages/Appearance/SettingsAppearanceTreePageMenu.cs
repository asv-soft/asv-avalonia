using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class SettingsAppearanceTreePageMenu : TreePage
{
    public SettingsAppearanceTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsAppearanceViewModel.PageId,
            RS.SettingsAppearanceViewModel_Name,
            MaterialIconKind.ThemeLightDark,
            SettingsAppearanceViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
