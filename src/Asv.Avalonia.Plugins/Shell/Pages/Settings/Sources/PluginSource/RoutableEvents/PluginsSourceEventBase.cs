using Asv.Common;
using Asv.Modeling;

namespace Asv.Avalonia.Plugins;

public abstract class PluginsSourceEventBase(PluginsSourceViewModel source, IPluginServerInfo info)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public IPluginServerInfo ServerInfo => info;
}
