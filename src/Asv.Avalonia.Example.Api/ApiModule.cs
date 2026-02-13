using System.Composition.Hosting;
using Asv.Avalonia.GeoMap;

namespace Asv.Avalonia.Example.Api;

public sealed class ApiModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.Example.Api";
    public static ApiModule Instance { get; } = new();

    private ApiModule() { }

    public string ModuleName => Name;
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromApi(
        this ContainerConfiguration containerConfiguration
    )
    {
        return containerConfiguration.WithAssemblies([typeof(ApiModule).Assembly]);
    }
}
