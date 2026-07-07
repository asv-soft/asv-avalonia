using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class SettingsPluginsTreePageMenu : TreePageMenuItem
{
    public SettingsPluginsTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsPluginsSourcesViewModel.PageId,
            RS.SettingsPluginsSourcesViewModel_Name,
            MaterialIconKind.Cloud,
            new NavId(SettingsPluginsSourcesViewModel.PageId),
            new NavId(PluginSettingsTreePageMenu.PageId)
        ) { }
}
