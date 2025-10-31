namespace Asv.Avalonia.Plugins;

public class PluginServer(
    string name,
    string sourceUri,
    string? username = null,
    string? password = null
)
{
    public string Name => name;
    public string SourceUri => sourceUri;
    public string? Username => username;
    public string? Password => password;
}
