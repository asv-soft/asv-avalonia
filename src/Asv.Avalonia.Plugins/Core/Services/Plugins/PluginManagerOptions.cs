namespace Asv.Avalonia.Plugins;

public class PluginManagerOptions
{
    public const string Section = "Plugins";
    public required string Salt { get; set; }
    public required string PluginDirectory { get; set; }
    public required string ApiPackageId { get; set; }
    public required string NugetPluginPrefix { get; set; }
    public required string ApiVersion { get; set; }
    public required string NugetDirectory { get; set; }
    public required string NugetCacheDirectory { get; set; }
    public List<PluginServer> DefaultServers { get; set; } = [];
}
