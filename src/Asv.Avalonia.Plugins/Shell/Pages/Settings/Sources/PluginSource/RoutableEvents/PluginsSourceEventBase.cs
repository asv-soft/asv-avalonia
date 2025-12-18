using Asv.Common;

namespace Asv.Avalonia.Plugins;

public abstract class PluginsSourceEventBase(PluginsSourceViewModel source, IPluginServerInfo info)
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public IPluginServerInfo ServerInfo => info;
}
