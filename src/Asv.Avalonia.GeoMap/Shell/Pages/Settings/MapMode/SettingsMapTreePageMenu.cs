using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class SettingsMapTreePageMenu : TreePage
{
    public SettingsMapTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsMapViewModel.SubPageId,
            "Map Settings",
            MaterialIconKind.Map,
            SettingsMapViewModel.SubPageId,
            SettingsAppearanceViewModel.PageId,
            loggerFactory
        ) { }
}
