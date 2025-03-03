using Asv.Common;

namespace Asv.Avalonia;

public class BuilderPluginManagerConfig
{
    public const string Section = "PluginManager";
    public required string ApiPackageName { get; set; }
    public required SemVersion ApiVersion { get; set; }
    public required string NugetPluginPrefix { get; set; }
    public List<PluginServer> Servers { get; } = [];
}
