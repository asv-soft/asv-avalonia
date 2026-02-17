using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class SettingsCommandTreePageMenu : TreePage
{
    public SettingsCommandTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsUnitsViewModel.PageId,
            RS.SettingsUnitsViewModel_Name,
            MaterialIconKind.TemperatureCelsius,
            SettingsUnitsViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
