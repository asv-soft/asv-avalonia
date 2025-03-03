using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly ILogger _logger;
    private readonly string _pluginFolder;

    public PluginAssemblyLoadContext(
        string pluginPath,
        string nugetPluginName,
        IList<Assembly> containerCfg,
        ILoggerFactory loggerFactory
    )
    {
        _logger = loggerFactory.CreateLogger(Path.GetFileNameWithoutExtension(pluginPath));
        _pluginFolder = Path.GetFullPath(pluginPath);
        foreach (
            var file in Directory.EnumerateFiles(
                pluginPath,
                nugetPluginName + "*.dll",
                SearchOption.AllDirectories
            )
        )
        {
            var fullPath = Path.GetFullPath(file);
            try
            {
                containerCfg.Add(LoadFromAssemblyPath(fullPath));
            }
            catch (Exception e)
            {
                _logger.ZLogError(e, $"Error load plugin assembly {fullPath}");
            }
        }
    }

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
            _logger.ZLogInformation(
                $"Load assembly '{assemblyName.Name}' from plugin folder {fullPath}"
            );
            return LoadFromAssemblyPath(fullPath);
        }

        return null;
    }
}
