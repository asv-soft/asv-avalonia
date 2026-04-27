using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class SettingsCommandTreePageMenu : TreePage
{
    public SettingsCommandTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsCommandListViewModel.PageId,
            RS.SettingsCommandListViewModel_Name,
            MaterialIconKind.TemperatureCelsius,
            new NavId(SettingsCommandListViewModel.PageId),
            NavId.Empty,
            loggerFactory
        ) { }
}
