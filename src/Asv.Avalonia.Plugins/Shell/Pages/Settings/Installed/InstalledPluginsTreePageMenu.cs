using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

public class InstalledPluginsTreePageMenu : TreePage
{
    public InstalledPluginsTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            InstalledPluginsPageViewModel.PageId,
            RS.InstalledPluginsPageViewModel_Title,
            MaterialIconKind.Plugin,
            new NavId(InstalledPluginsPageViewModel.PageId),
            new NavId(PluginSettingsTreePageMenu.PageId)
        ) { }
}
