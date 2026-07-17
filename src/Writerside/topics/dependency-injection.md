# Dependency Injection

Asv.Avalonia uses the standard `Microsoft.Extensions.Hosting` and
`Microsoft.Extensions.DependencyInjection` stack. The application creates an
`AppHost` during Avalonia startup, configures services through an
`IHostApplicationBuilder`, builds the host, and then uses the resulting service
provider through `AppHost.Instance.Services`.

## Application startup

The entry point for Asv.Avalonia composition is `UseAsv`:

```C#
public static AppBuilder BuildAvaloniaApp()
{
    return AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .UseAsv(builder =>
        {
            builder
                .EnableLogging()
                .RegisterCore(core =>
                {
                    core.RegisterControls();
                    core.RegisterServices(services =>
                    {
                        services
                            .RegisterAppInfo()
                            .RegisterAppPath()
                            .RegisterDialogs()
                            .RegisterExtensions()
                            .RegisterLocalizationService()
                            .RegisterShellHost()
                            .RegisterThemeService()
                            .RegisterViewLocator();
                    });
                })
                .RegisterDesktopShell()
                .RegisterModuleGeoMap()
                .RegisterModuleIo();
        });
}
```

`UseAsv` creates an `AppHost.Builder`, which implements `IHostApplicationBuilder`.
All framework, application, and module registrations run against this builder.
After Avalonia platform services are ready, Asv.Avalonia builds the host, starts
it, and stores it in `AppHost.Instance`.

The application class should inherit from `AsvApplication`. During initialization
it resolves the `ViewLocator`, `IShellHost`, and `IShell` from
`AppHost.Instance.Services` and connects the shell to the active Avalonia
lifetime.

## Registration layers

Registration methods are grouped by feature area and exposed as fluent extension
methods over `IHostApplicationBuilder` or a nested feature builder.

The top-level framework registration is:

```C#
builder.RegisterDefault();
```

It currently enables logging and registers the core framework services and
controls. More explicit applications usually compose only the pieces they need:

```C#
builder.RegisterCore(core =>
{
    core.RegisterServices(services =>
    {
        services.RegisterAppInfo();
        services.RegisterViewLocator();
    });

    core.RegisterControls(controls =>
    {
        controls.RegisterPropertyEditor();
    });
});
```

Each registration method follows the same pattern:

1. If no callback is provided, it calls its local `RegisterDefault()` method.
2. If a callback is provided, only the registrations inside that callback are
   applied.
3. The method returns the parent builder so registrations can be chained.

This means an empty callback intentionally disables the defaults for that layer:

```C#
builder.RegisterCore(_ => { });
```

Use this only when the application provides every required service itself.

## Services, views, and pages

Feature registration methods eventually write to `builder.AppBuilder.Services`.
The framework uses the same lifetimes as regular .NET applications:

- Singleton services for application-wide services such as `IThemeService`,
  `IExtensionService`, `IUnitService`, `IShellHost`, and `ViewLocator`.
- Transient services for view models, views, shell windows, extensions, menu
  items, status items, and dialog instances.
- Hosted services for background features such as restart execution,
  single-instance argument forwarding, and file association handling.
- Keyed services when one contract has many implementations selected by an id.

Pages are a good example of a composite registration:

```C#
builder.Shell.Pages.Register<ReportsPageViewModel, ReportsPageView>(
    ReportsPageViewModel.PageId
);
```

This registers two things:

- a keyed `ViewModelFactoryDelegate<IPage, IPageContext>` for the page id;
- a keyed Avalonia `Control` mapping in `ViewLocator` for the page view model.

Because the view mapping is resolved by `ViewLocator`, applications that register
pages or custom views must also register the `ViewLocator` service. The default
core services do this through `RegisterViewLocator()`.

## Module registration

Modules follow the same builder pattern as the core framework. A module normally
provides one top-level extension method on `IHostApplicationBuilder`, a nested
builder that implements `IDependencyBuilder`, and smaller `RegisterCore`,
`RegisterShell`, `RegisterServices`, `RegisterControls`, or `RegisterPages`
methods.

```C#
public static class ReportsRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ModuleReports => new(builder);

        public IHostApplicationBuilder RegisterModuleReports(
            Action<Builder>? configure = null
        )
        {
            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder RegisterDefault()
        {
            this.RegisterCore();
            this.RegisterShell();
            return this;
        }
    }
}
```

Inside the module, register concrete dependencies through the existing framework
builders whenever possible:

```C#
public static class ReportsPagesRegistrations
{
    extension(ReportsRegistrations.Builder builder)
    {
        public ReportsRegistrations.Builder RegisterShell()
        {
            builder.AppBuilder.Shell.Pages.Register<ReportsPageViewModel, ReportsPageView>(
                ReportsPageViewModel.PageId
            );

            builder.AppBuilder.Extensions.Register<IHomePage, ReportsHomePageExtension>();
            return builder;
        }
    }
}
```

Use direct `builder.AppBuilder.Services.AddSingleton`, `AddTransient`,
`AddKeyedSingleton`, or `AddKeyedTransient` calls for services that do not have a
dedicated framework registration helper.

## Lightweight builders

In theory, an application, module, or plugin can expose a lightweight builder
with only one dependency registration method:

```C#
public static class ReportsRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder RegisterReports()
        {
            builder.Services.AddSingleton<IReportService, ReportService>();
            builder.Shell.Pages.Register<ReportsPageViewModel, ReportsPageView>(
                ReportsPageViewModel.PageId
            );

            return builder;
        }
    }
}
```

This can be acceptable for very small features, but it should not be the default
shape for a module that is expected to grow. A single registration method hides
the internal structure of the feature and gives the host application no clean way
to select only the services, pages, controls, or shell integrations it needs.

The main drawbacks are:

- The host application cannot customize composition without copying or replacing
  the whole registration method.
- Optional features become harder to disable because every dependency is coupled
  to one entry point.
- Tests have less control over the dependency graph and often need to register
  more services than the scenario actually requires.
- Future changes tend to turn the method into a large mixed list of unrelated
  registrations.
- Breaking changes are harder to avoid because any new dependency can affect all
  applications that call the single method.
- Design-time and runtime registrations are harder to separate cleanly.

Prefer the layered builder pattern for modules and plugins that expose more than
one feature area. Keep the single-method style for small, stable, internal
features where customization is not required.

## Builder shortcuts

When a specific part of the builder is used from many places, it is useful to add
a small shortcut extension. A shortcut does not register anything by itself; it
only gives convenient access to an existing nested builder.

For example, the framework exposes shortcuts such as:

```C#
extension(IHostApplicationBuilder builder)
{
    public ViewLocatorRegistrations.Builder ViewLocator =>
        builder.Core.Services.ViewLocator;

    public PagesRegistrations.Builder Pages =>
        builder.Shell.Pages;
}
```

This allows code to use:

```C#
builder.ViewLocator.RegisterViewFor<ReportsPageViewModel, ReportsPageView>();
builder.Pages.Register<ReportsPageViewModel, ReportsPageView>(
    ReportsPageViewModel.PageId
);
```

instead of repeatedly spelling the full path to the nested builder.

Create shortcut files only when the same nested builder is needed often from
different parts of the application, module, or plugin. Keep shortcuts thin and
predictable:

- Return an existing builder; do not perform registrations in the shortcut
  property.
- Use names that match the target feature area, such as `ViewLocator`, `Pages`,
  `Settings`, or `Extensions`.
- Place shortcuts close to the registration type they expose so ownership stays
  clear.
- Avoid adding shortcuts for one-off calls because they increase the public
  composition surface without reducing real duplication.

## Design-time registrations

Some framework services switch implementation when the host runs in Avalonia
design mode. Registration methods check `builder.AppBuilder.IsDesignTimeEnvironment`
and register a lightweight or null implementation instead of the runtime service.

Examples include dialog, extension, localization, search, theme, file
association, app arguments, and device-manager services. Keep this behavior in
mind when adding services used by views in the designer: the registration should
avoid filesystem, network, or platform work that can break design-time loading.

## Plugin registration

The plugins module also uses the same DI builder. It registers the plugin
bootloader and plugin manager as services. The bootloader adds a post-configure
callback through `AddPostConfigureCallbacks`.

Post-configure callbacks run immediately before the host is built. The plugin
bootloader uses that phase to call every loaded plugin entry point:

```C#
public interface IPluginAppBuilder
{
    void Register(IHostApplicationBuilder builder);
}
```

A plugin registers dependencies exactly like an application or module:

```C#
public class PluginEntryPoint : IPluginAppBuilder
{
    public void Register(IHostApplicationBuilder builder)
    {
        builder.RegisterModuleReports();
    }
}
```

Use post-configure only for composition steps that must run after the main
application registration chain has completed. Regular services, views, pages, and
extensions should be registered directly in the normal builder chain.

## Recommended rules

- Register dependencies during `UseAsv`, before the host is built.
- Prefer existing Asv.Avalonia registration helpers over direct service
  collection calls.
- Keep module defaults cohesive but allow applications to override them with a
  configured callback.
- Avoid single-method builders for modules that need long-term customization.
- Add builder shortcuts only when they make frequently used nested builders
  easier to access from several places.
- Use keyed registrations for extensible contracts selected by an id, such as
  pages, views, unit items, status items, menu items, and port view models.
