using System.Composition;

namespace Asv.Avalonia;

/// <summary>
/// This attribute is used to find a matching plugin entry points
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class)]
public class PluginEntryPointAttribute : ExportAttribute, IPluginMetadata
{
    public PluginEntryPointAttribute(string name, params string[] dependency)
        : base(typeof(IPluginEntryPoint))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Dependency = dependency;
    }

    public string[] Dependency { get; }
    public string Name { get; }
}
