using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia.Plugins;

public class PluginSettingsTreePageMenu()
    : TreePage(PageId, "Plugins", MaterialIconKind.Plugin, NavId.Empty, NavId.Empty)
{
    public const string PageId = "plugins";
}
