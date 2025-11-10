using System.Reflection;

namespace Asv.Avalonia.Plugins;

public sealed class PluginManagerModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.Plugins";
    public static readonly IExportInfo Instance = new PluginManagerModule();
    public static readonly IEnumerable<Assembly> Assemblies =
    [
        typeof(PluginManagerModule).Assembly,
    ];

    private PluginManagerModule() { }

    public string ModuleName => Name;
}
