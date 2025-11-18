namespace Asv.Avalonia.Example.Plugin.PluginExample;

public class PluginExampleInfo : IExportInfo
{
    public const string Name = "Asv.Avalonia.Plugin.Example";
    public static IExportInfo Instance { get; } = new PluginExampleInfo();

    private PluginExampleInfo() { }

    public string ModuleName => Name;
}
