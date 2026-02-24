using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class SettingsUnitTreePageMenu : TreePage
{
    public SettingsUnitTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsUnitsViewModel.PageId,
            RS.SettingsUnitsViewModel_Name,
            MaterialIconKind.KeyboardSettings,
            SettingsUnitsViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
