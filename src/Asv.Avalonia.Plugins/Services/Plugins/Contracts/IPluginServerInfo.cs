namespace Asv.Avalonia.Plugins;

public interface IPluginServerInfo
{
    public string Name { get; }
    public string SourceUri { get; }
    public string? Username { get; }
}
