using Asv.Avalonia.Example.Api;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

public class App : ShellHost
{
    public App()
        : base(cfg =>
        {
            cfg.WithDependenciesFromSystemModule();
            cfg.WithDependenciesFromIoModule();
            cfg.WithDependenciesFromPluginManagerModule();
            cfg.WithDependenciesFromGeoMapModule();
            cfg.WithDependenciesFromApi();
        }) { }

    protected override void LoadXaml()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
