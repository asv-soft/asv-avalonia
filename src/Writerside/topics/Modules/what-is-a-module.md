# What is a module?

A module is a feature package that extends the Asv.Avalonia host with services, views, pages, dialogs, and registrations. The application chooses its modules during host configuration. A module does not require a separately downloaded plugin or a special entry-point interface.

The framework includes these optional packages:

| Package | Purpose | Registration |
|---|---|---|
| `Asv.Avalonia.GeoMap` | Tile maps, geographic markers, coordinate dialogs, and map settings | `RegisterModuleGeoMap()` |
| `Asv.Avalonia.IO` | Connections, device discovery, live packet inspection, and file inspection | `RegisterModuleIo()` |
| `Asv.Avalonia.Plugins` | NuGet plugin management and startup loading | `RegisterModulePlugins()` |

Add the relevant package reference to your application. In its existing host configuration callback, after the core framework and desktop shell registrations, register the modules you need:

```c#
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Microsoft.Extensions.Hosting;

static void RegisterFeatures(IHostApplicationBuilder builder)
{
    builder.RegisterModuleGeoMap();
    builder.RegisterModuleIo();
    builder.RegisterModulePlugins();
}
```

This is a registration fragment, not a replacement for application startup. The complete desktop bootstrap is in `Asv.Avalonia.Example.Desktop/Program.cs`. Plugin loading also requires application-specific API and package-prefix configuration; see [Plugins module](plugins-module.md).

## Customize registration explicitly

Each module exposes a builder with `RegisterDefault()`, `RegisterCore()`, and `RegisterShell()`. The default registration combines the module's core and shell registrations.

A callback passed to `RegisterModuleGeoMap`, `RegisterModuleIo`, or `RegisterModulePlugins` **replaces** the default callback. It does not run after the defaults automatically. The same convention applies to nested registration methods that use a default callback.

For example, explicitly retain all GeoMap defaults when supplying a callback:

```c#
builder.RegisterModuleGeoMap(module =>
{
    module.RegisterDefault();
    // Add application-specific registrations here.
});
```

To deliberately select only part of a module, call the required registration methods instead. Check their dependencies before omitting shell or core components. Avoid registering the same defaults twice: collection registrations can accumulate multiple instances.

The `ModuleGeoMap`, `ModuleIo`, and `ModulePlugins` properties expose builders for extending an already configured module. Accessing a builder property does not register that module's defaults.

## Module versus dynamic plugin

A module is linked into the application and configured by its host. A dynamic plugin is an installed package loaded by the Plugins module at startup. Its `IPluginAppBuilder.Register(IHostApplicationBuilder)` entry point can register pages, extensions, and services using the same builders as the application.

Use a module for functionality shipped with the application. Use dynamic plugins when an application intentionally supports installing separately packaged extensions with a shared API contract.

Continue with [GeoMap](geomap-module.md), [IO](io-module.md), or [Plugins](plugins-module.md).

See the running application screenshots in [Shell and navigation](shell-and-navigation.md),
[GeoMap](geomap-module.md), [IO](io-module.md), and [Plugins](plugins-module.md).
