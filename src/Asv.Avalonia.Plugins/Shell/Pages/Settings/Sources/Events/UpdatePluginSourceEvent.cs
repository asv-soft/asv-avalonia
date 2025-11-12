namespace Asv.Avalonia.Plugins;

public sealed class UpdatePluginSourceEvent(PluginSourceViewModel source)
    : AsyncRoutedEvent(source, RoutingStrategy.Tunnel)
{
    public PluginSourceViewModel Page => source;
}
