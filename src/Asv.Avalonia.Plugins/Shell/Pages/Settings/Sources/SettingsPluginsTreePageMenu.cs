using Asv.Common;
using Asv.Modeling;
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
            new NavId(SettingsPluginsSourcesViewModel.PageId),
            NavId.Empty,
            loggerFactory
        ) { }
}
