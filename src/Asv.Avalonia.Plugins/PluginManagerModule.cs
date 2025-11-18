using System.Composition.Hosting;
using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

public sealed class PluginManagerModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.Plugins";
    public static IExportInfo Instance { get; } = new PluginManagerModule();

    private PluginManagerModule() { }

    public string ModuleName => Name;
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromPluginManagerModule(
        this ContainerConfiguration containerConfiguration
    )
    {
        if (Design.IsDesignMode)
        {
            containerConfiguration.WithExport(NullPluginManager.Instance);
            return containerConfiguration.WithAssemblies([typeof(PluginManagerModule).Assembly]);
        }

        var exceptionTypes = new List<Type>();
        if (AppHost.Instance.GetServiceOrDefault<IPluginManager>() is { } pluginManager)
        {
            containerConfiguration.WithExport(pluginManager);
            containerConfiguration.WithAssemblies(pluginManager.PluginsAssemblies.Distinct());
        }
        else
        {
            exceptionTypes.AddRange(typeof(PluginManagerModule).Assembly.GetTypes());
        }

        var pluginManagerTypes = typeof(PluginManagerModule)
            .Assembly.GetTypes()
            .Except(exceptionTypes);

        return containerConfiguration.WithParts(pluginManagerTypes);
    }
}
