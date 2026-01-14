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
*   **[Localization Service](localization-service.md):** Manages language switching and string translations.
*   **[Theme Service](theme-service.md):** Controls the visual appearance (Dark/Light themes, accent colors).

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
[Shared] // You may makes it a singleton
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

## Service Registration

### Standard Registration (Automatic)

By default, Asv.Avalonia relies on MEF's convention-based registration. This is the method for most services.

1. **Mark the Class**: Add `[Export]` and `[Shared]` attributes to your service implementation.

2. **Scan the Assembly**: In `App.axaml.cs`, the framework scans your assembly for these attributes.

If you create a service class, use MEF attributes on it:
```C#
// In your service file:
[Export(typeof(IMyCustomService))]
[Shared]
public class MyCustomService : AsyncDisposableOnce, IMyCustomService { ... }

// In App.axaml.cs:
// The container will automatically find MyCustomService because it has the [Export] attribute
containerCfg
    .WithDependenciesFromTheApp(this) // Scans the current assembly
    .WithDefaultConventions(conventions);
```

### Conditional Registration (Optional Services)

Sometimes a service should only be registered under specific conditions (e.g., specific configuration settings or OS environment). 
There are two ways to achieve this.

#### Method 1: The Exclusion Strategy (Recommended)

This is the preferred approach. You keep the MEF attributes (`[Export]`, `[Shared]`) on your class, treating it as a standard service. 
However, during the container setup, you explicitly exclude it from registration based on your condition.

```C#
// 1. Prepare a list of types to exclude
var exceptionTypes = new List<Type>();

// 2. Check your condition
var options = AppHost.Instance.GetService<IOptions<LoggerOptions>>().Value;
if (options.ViewerEnabled == false)
{
    // If the feature is disabled, exclude the implementation and related views
    exceptionTypes.AddRange(new[] 
    {
        typeof(LogViewerViewModel),
        typeof(LogViewerView),
        // ... other related services
    });
}

// 3. Get all types from the target assembly (e.g., SystemModule) and subtract the exceptions.
var systemTypes = typeof(SystemModule).Assembly.GetTypes().Except(exceptionTypes);

// 4. Register the filtered list
containerConfiguration.WithParts(systemTypes);
```

#### Method 2: Manual Registration

Alternatively, you can remove the MEF attributes (`[Export]`, `[Shared]`) from the service class entirely. 
Since the scanner won't pick it up automatically, you must manually register the instance in the container.

Use this method when you need to register an instance that was created outside the container (e.g., passed from `Program.cs` or a legacy system).

```C#
// No [Export] attribute on the class
public class AppPath : IAppPath { ... }

// In App.axaml.cs:
if (AppHost.Instance.GetServiceOrDefault<IAppPath>() is { } appPath)
{
    // Manually add the specific instance to the container
    containerConfiguration.WithExport(appPath);
}
```
