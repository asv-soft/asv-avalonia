using System.Reflection;
using System.Runtime.Loader;

namespace Asv.Avalonia.Plugins;

public class PluginAssemblyLoadContext(string pluginPath) : AssemblyLoadContext
{
    public static PluginAssemblyLoadContext Create(
        string folder,
        string assemblyPrefix,
        IList<Assembly> assembyRegistry
    )
    {
        var context = new PluginAssemblyLoadContext(folder);
        foreach (
            var file in Directory.EnumerateFiles(
                folder,
                $"{assemblyPrefix}*.dll",
                SearchOption.AllDirectories
            )
        )
        {
            assembyRegistry.Add(context.LoadFromAssemblyPath(Path.GetFullPath(file)));
        }
        return context;
    }

    private readonly string _pluginFolder = Path.GetFullPath(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // if assembly already loaded at main context => return null (it will be loaded by main context)
        if (Default.Assemblies.Any(x => x.GetName().Name == assemblyName.Name))
        {
            return null;
        }

        // this plugin wants to load assembly, but main context not contains it yet => try to load from main context
        try
        {
            return Default.LoadFromAssemblyName(assemblyName);
        }
        catch (Exception e)
        {
            // if we here, it's mean that assembly not found at main context => try load from plugin folder
            /*_logger.ZLogWarning(e, $"Assembly {assemblyName.Name} not found at main context");*/
        }

        // if we here, it's mean that assembly not found at main context => try to load from plugin folder
        foreach (
            var file in Directory.GetFiles(
                _pluginFolder,
                assemblyName.Name + ".dll",
                SearchOption.AllDirectories
            )
        )
        {
            var fullPath = Path.GetFullPath(file);
            return LoadFromAssemblyPath(fullPath);
        }

        return null;
    }
}
