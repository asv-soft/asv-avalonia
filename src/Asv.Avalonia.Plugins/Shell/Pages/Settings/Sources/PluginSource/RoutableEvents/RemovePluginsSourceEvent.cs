namespace Asv.Avalonia.Plugins;

public sealed class RemovePluginsSourceEvent(PluginsSourceViewModel source, IPluginServerInfo info)
    : PluginsSourceEventBase(source, info) { }
