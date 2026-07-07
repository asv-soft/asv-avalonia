using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

public class PluginsMarketTreePageMenu : TreePage
{
    public PluginsMarketTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            PluginsMarketPageViewModel.PageId,
            RS.PluginsMarketPageViewModel_Title,
            MaterialIconKind.Store,
            new NavId(PluginsMarketPageViewModel.PageId),
            new NavId(PluginSettingsTreePageMenu.PageId)
        ) { }
}
