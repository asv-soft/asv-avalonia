namespace Asv.Avalonia.Plugins;

public abstract class PluginSourceEventBase(PluginSourceViewModel source, IPluginServerInfo info)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
{
    public IPluginServerInfo ServerInfo => info;
}
