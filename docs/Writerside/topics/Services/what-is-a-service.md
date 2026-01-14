# What is a Service?

## Overview

In Asv.Avalonia, a Service is a specialized, usually singleton component that provides shared logic and infrastructure across the entire application. While ViewModels handle the logic for specific UI pieces, Services handle "global" concerns that don't belong to any single view.

## Core Concept

Services are the backbone of your application's infrastructure. They are designed to be:

*   **Singletons:** Most services exist as a single instance throughout the application's lifecycle.
*   **Decoupled:** They don't know about the UI (Views). They provide data or functionality that ViewModels can consume.
*   **Injected:** Services are managed by the [MEF (Managed Extensibility Framework)](https://learn.microsoft.com/en-us/dotnet/framework/mef/) and are injected into other components via constructors.

## Key Characteristics

### 1. Lifecycle Management
Services can inherit from `AsyncDisposableOnce`. This allows them to perform asynchronous cleanup when the application shuts down, such as closing database connections or saving state.

### 2. Global State
Services are the perfect place to store state that needs to persist as the user navigates between pages. For example, the `NavigationService` keeps track of the navigation history, and the `ThemeService` remembers whether the user prefers Dark or Light mode.

### 3. Configuration
Some services need settings. By inheriting from `ServiceWithConfigBase<TConfig>`, a service automatically gains the ability to load and save its state using the framework's configuration system. See our [`LocalizationService`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Services/Localization/LocalizationService.cs) for the example.

## Common Built-in Services

The framework provides several essential services out of the box. For example:

*   **[Navigation Service](navigation-service.md):** Manages page switching, back/forward history, and focus tracking.
*   **[Command Service](command-service.md):** Centralizes command execution, global hotkeys, and Undo/Redo history.
*   **Localization Service:** Manages language switching and string translations.
*   **Theme Service:** Controls the visual appearance (Dark/Light themes, accent colors).

## When to Create a Service?

You should consider creating a service when you have logic that:
1.  Needs to be accessed from multiple ViewModels.
2.  Manages a global resource (e.g., a hardware connection, a database, or a web API).
3.  Needs to stay alive even when the current page is closed.
4.  Handles cross-cutting concerns like logging, caching, or user preferences.

## How to Define a Service

A typical service consists of an interface and an implementation marked with MEF attributes:

```C#
// 1. Define the interface
public interface IMyCustomService
{
    void DoSomething();
}

// 2. Implement the service
[Export(typeof(IMyCustomService))]
[Shared] // Makes it a singleton
public class MyCustomService : AsyncDisposableOnce, IMyCustomService
{
    public void DoSomething() 
    {
        // Your logic here
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Your Dispose logic here
        }

        base.Dispose(disposing);
    }
}
```

To use it, simply request it in a constructor:

```C#
public class MyViewModel: RoutableViewModel
{
    private readonly IMyCustomService _service;
    
    [ImportingConstructor]
    public MyViewModel(IMyCustomService service, ILoggerFactory loggerFactory)
        : base("view-model-id", loggerFactory)
    {
        _service = service;
        _service.DoSomething();
    }
}
```

## Service Registration & Resolution

Asv.Avalonia uses a two-stage initialization process to register and resolve services.

### 1. System Initialization (Program.cs)
First, the `AppHost` initializes low-level system services like logging, configuration, and the PluginManager. This happens in `Program.cs`.

```C#
builder.UsePluginManager(options => {
    options.WithApiPackage("Asv.Avalonia.Example.Api", SemVersion.Parse("1.0.0"));
    options.WithPluginPrefix("Asv.Avalonia.Example.Plugin.");
});
```

### 2. Container Composition (App.axaml.cs)
Next, the application creates a MEF container in `App.axaml.cs`. This container aggregates:
*   System services from `AppHost` (forwarded to MEF).
*   Services from loaded Plugins.
*   Application-specific services (ViewModels, Views).

```C#
// App.axaml.cs
var containerCfg = new ContainerConfiguration();
containerCfg
    .WithDependenciesFromSystemModule()        // Forward system services
    .WithDependenciesFromPluginManagerModule() // Load plugin assemblies
    .WithDependenciesFromTheApp(this);         // Load app components

_container = containerCfg.CreateContainer();
```

### 3. Registration Methods

There are two common ways to register services in the MEF container:

#### Method 1: Using MEF Attributes

Most application services use MEF attributes directly. This is the standard approach for services created within your application.

```C#
[Export(typeof(INavigationService))]
[Shared]
public class NavigationService : INavigationService
{
    // Implementation
}
```

> **Important:** MEF only discovers services from assemblies that are registered in the container. If your service is in a separate module, ensure that assembly is added in `App.axaml.cs` (e.g., via `WithAssemblies()` or custom method like a `WithDependenciesFromYourModule()`).

#### Method 2: Explicit Registration (for AppHost Services)

Some services don't use MEF attributes. Instead, they are registered in `AppHost` (Program.cs) and then explicitly forwarded to MEF in the module's configuration method.

```C#
// In Program.cs - registered in AppHost
builder.UseAppPath(opt => opt.WithRelativeFolder("data"));

// In SystemModule.cs - forwarded to MEF
if (AppHost.Instance.GetServiceOrDefault<IAppPath>() is { } appPath)
{
    containerConfiguration.WithExport(appPath);
}
```

This approach allows services to be optionally configured in `AppHost` and conditionally added to the MEF container.
