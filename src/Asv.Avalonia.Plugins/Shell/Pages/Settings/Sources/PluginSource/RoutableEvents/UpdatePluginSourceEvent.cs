namespace Asv.Avalonia.Plugins;

public sealed class UpdatePluginSourceEvent(
    PluginSourceViewModel source,
    IPluginServerInfo info,
    PluginServer server
) : PluginSourceEventBase(source, info)
{
    public PluginServer Server => server;
}
