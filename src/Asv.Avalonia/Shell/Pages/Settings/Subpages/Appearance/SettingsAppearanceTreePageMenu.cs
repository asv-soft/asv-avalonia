using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class AppearanceSettingTreePageMenu : TreePage
{
    public AppearanceSettingTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsAppearanceViewModel.PageId,
            RS.SettingsAppearanceViewModel_Name,
            MaterialIconKind.ThemeLightDark,
            SettingsAppearanceViewModel.PageId,
            NavId.Empty,
            loggerFactory
        ) { }
}
