namespace Asv.Avalonia.Plugins;

public sealed class RemovePluginSourceEvent(PluginSourceViewModel source, IPluginServerInfo info)
    : PluginSourceEventBase(source, info) { }
