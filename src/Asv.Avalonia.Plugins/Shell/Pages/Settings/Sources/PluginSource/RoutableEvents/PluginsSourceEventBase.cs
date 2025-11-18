namespace Asv.Avalonia.Plugins;

public abstract class PluginsSourceEventBase(PluginsSourceViewModel source, IPluginServerInfo info)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
{
    public IPluginServerInfo ServerInfo => info;
}
