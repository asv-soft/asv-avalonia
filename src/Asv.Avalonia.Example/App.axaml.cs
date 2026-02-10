using System.Composition.Hosting;
using System.Reflection;
using Asv.Avalonia.Example.Api;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

public class App : AppBase
{
    public App()
        : base(
            new ContainerConfiguration()
                .WithDependenciesFromSystemModule()
                .WithDependenciesFromIoModule()
                .WithDependenciesFromPluginManagerModule()
                .WithDependenciesFromGeoMapModule()
                .WithDependenciesFromApi()
                .WithDependenciesFromTheAssembly(Assembly.GetExecutingAssembly())
        ) { }

    protected override void LoadXaml()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
