namespace Asv.Avalonia.Plugins;

public sealed class NullPluginServerInfo : IPluginServerInfo
{
    public static IPluginServerInfo Instance { get; } = new NullPluginServerInfo();

    private NullPluginServerInfo() { }

    public string Name => "Server Name";
    public string SourceUri => "https://nuget.pkg.github.com/asv-soft/index.json";
    public string? Username => "user";
}
