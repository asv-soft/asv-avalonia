using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class SettingsUnitTreePageMenu : TreePage
{
    public SettingsUnitTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsCommandListViewModel.PageId,
            RS.SettingsCommandListViewModel_Name,
            MaterialIconKind.KeyboardSettings,
            SettingsCommandListViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
