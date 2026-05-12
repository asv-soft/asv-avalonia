using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Plugins;

public class PluginSettingsTreePageMenu : TreePage
{
    public const string PageId = "plugins";

    public PluginSettingsTreePageMenu(ILoggerFactory loggerFactory)
        : base(PageId, "Plugins", MaterialIconKind.Plugin, NavId.Empty, NavId.Empty, loggerFactory)
    { }
}
