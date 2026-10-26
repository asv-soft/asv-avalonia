# Plugins module

<img src="guide-plugins.png" dark-src="guide-plugins.png" alt="Installed Plugins settings page" thumbnail="true"/>

Open Home → Settings → Plugins → Installed Plugins. This example has no installed plugin selected; the page still exposes filtering and manual installation controls.

`Asv.Avalonia.Plugins` provides package sources, plugin search/install UI, installed-plugin status, and startup loading. A plugin is a NuGet package that depends on the application's shared API package and contains an `IPluginAppBuilder` entry point.

See [What is a module?](what-is-a-module.md) for the distinction between linked feature modules and separately installed plugins.

## Configure the host

The application must choose its API package identity/version and plugin naming prefix. The following registration fragment uses the example application's actual API assembly and package prefix:

```c#
using Asv.Avalonia.Example.Api;
using Asv.Avalonia.Plugins;
using Asv.Common;

builder.RegisterModulePlugins(module =>
{
    module.RegisterCore(core => core.RegisterServices(services =>
    {
        services.RegisterPluginBootloader(loader =>
        {
            loader.WithApiPackage(typeof(Command1).Assembly);
            loader.Options.PluginAssemblyPrefix =
                "Asv.Avalonia.Example.Plugin.";
        });
        services.RegisterPluginManager(manager => manager
            .WithApiPackage("Asv.Avalonia.Example.Api", SemVersion.Parse("1.0.0"))
            .WithPluginPrefix("Asv.Avalonia.Example.Plugin."));
    }));
    module.RegisterShell();
});
```

Replace these example identities with your application's shared API assembly, API version, and plugin prefix. `Command1` belongs to the example API project; it is not a framework type. The assembly supplied to `WithApiPackage(Assembly)` must carry an informational version that can be parsed as a semantic version.

The module callback replaces default registration. Here core services are deliberately selected, and `RegisterShell()` retains the default settings pages. Do not additionally call `RegisterDefault()` in this fragment, because it would register bootloader services again.

The manager's `WithPluginPrefix` controls package discovery; the bootloader's `PluginAssemblyPrefix` controls assembly selection. Configure both consistently. The bootloader defaults to `Asv.Avalonia.Plugins.`, which differs from the example package prefix.

## Implement an entry point

A minimal plugin entry point can register application services during startup:

```c#
using Asv.Avalonia.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public sealed class PluginEntryPoint : IPluginAppBuilder
{
    public void Register(IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<PluginGreeting>();
    }
}

public sealed class PluginGreeting
{
    public string Text => "Hello from the plugin";
}
```

The loader constructs concrete entry-point classes with `Activator.CreateInstance`, so use a public parameterless constructor. Register dependencies into the host inside `Register`; do not expect constructor injection into the entry point itself.

For a complete UI plugin, inspect `Asv.Avalonia.Example.Plugin.PluginExample`. Its `UsePluginExample()` registers `ExamplePageViewModel` with `ExamplePageView` and attaches `HomePagePluginExtension` to the home page. Its `PluginEntryPoint.Register` calls that method. The separate `Asv.Avalonia.Example.Api` project holds the shared application contract.

## Package and load

Reference the host's shared API package from the plugin package and use a package/assembly name matching the configured prefixes. The bootloader reads the API dependency from the package metadata and compares its semantic version precedence with the running API version. A mismatch is recorded as a loading error. This is stricter than merely assuming that any newer API is compatible.

By default the bootloader scans immediate subdirectories of `plugins` under the host content root. Each plugin directory must contain exactly one `.nupkg`. `PluginBootloaderOptions.AdditionalFolderPerPlugin` can add explicit development directories; missing directories are skipped.

The loader prepares plugin assemblies and registers their entry points through a host post-configuration callback. Installation is not live activation: restart the application to load newly installed packages. Plugin load contexts and their assemblies are application code, so install packages from sources you trust.

## Manage plugins in the shell

The default plugin settings include installed plugins, the market, and package sources. The manager starts with the NuGet v3 source `https://api.nuget.org/v3/index.json`; add another source through the UI or `WithServer(new PluginServer(name, uri))`.

The market searches packages using the configured API and package-prefix criteria. Installed-plugin status includes the API version and loading errors. Manual package installation is available through `IPluginManager.InstallManually`.

Uninstall marks a plugin for removal; the bootloader removes its directory during the next startup scan. `CancelUninstall` reverses that pending mark before removal. Installing or marking a package does not unload an already running assembly.

The manager also uses `nuget` and `nuget_cache` folders under the content root by default. Its folder settings and the bootloader's `RelativeFolder` are separate configuration points; keep plugin locations aligned when customizing them.
