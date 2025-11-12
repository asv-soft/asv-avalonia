namespace Asv.Avalonia.Plugins;

public sealed class RemovePluginSourceEvent(PluginSourceViewModel source)
    : AsyncRoutedEvent(source, RoutingStrategy.Tunnel)
{
    public PluginSourceViewModel Page => source;
}
