namespace Asv.Avalonia.Plugins;

public sealed class UpdatePluginsSourceEvent(
    PluginsSourceViewModel source,
    IPluginServerInfo info,
    PluginServer server
) : PluginsSourceEventBase(source, info)
{
    public PluginServer Server => server;
}
