using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

public class SettingsPluginsTreePageMenu : TreePage
{
    public SettingsPluginsTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsPluginsSourcesViewModel.PageId,
            RS.SettingsPluginsSourcesViewModel_Name,
            MaterialIconKind.Cloud,
            SettingsPluginsSourcesViewModel.PageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
