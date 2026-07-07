using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia.GeoMap;

public class SettingsGeoMapTreePageMenu()
    : TreePageMenuItem(
        SettingsGeoMapViewModel.PageId,
        RS.SettingsGeoMapViewModel_Name,
        MaterialIconKind.Map,
        new NavId(SettingsGeoMapViewModel.PageId),
        NavId.Empty
    );
