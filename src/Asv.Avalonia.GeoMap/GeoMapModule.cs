using System.Composition.Hosting;
using Avalonia.Controls;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.GeoMap;

public sealed class GeoMapModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.GeoMap";
    public static IExportInfo Instance { get; } = new GeoMapModule();

    private GeoMapModule() { }

    public string ModuleName => Name;
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromGeoMapModule(
        this ContainerConfiguration containerConfiguration
    )
    {
        if (Design.IsDesignMode)
        {
            return containerConfiguration.WithAssemblies([typeof(GeoMapModule).Assembly]);
        }

        // Here we use options instead of service existence check, because we get required service in the view
        var exceptionTypes = new List<Type>();
        var options = AppHost.Instance.GetService<IOptions<GeoMapOptions>>().Value;
        if (!options.IsEnabled)
        {
            exceptionTypes.AddRange(typeof(GeoMapModule).Assembly.GetTypes());
        }

        var geoMapTypes = typeof(GeoMapModule).Assembly.GetTypes().Except(exceptionTypes);
        return containerConfiguration.WithParts(geoMapTypes);
    }
}
