using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class SettingsGeoMapTreePageMenu : TreePage
{
    public SettingsGeoMapTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsGeoMapViewModel.PageId,
            RS.SettingsGeoMapViewModel_Name,
            MaterialIconKind.Map,
            SettingsGeoMapViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
