# What is a Service?

## Overview

In Asv.Avalonia, a Service is a specialized, usually singleton component that provides shared logic and infrastructure across the entire application. 
While ViewModels handle the logic for specific UI pieces, Services handle "global" concerns that don't belong to any single view.

## Core Concept

Services are the backbone of your application's infrastructure. They are designed to be:

*   **Singletons:** Most services exist as a single instance throughout the application's lifecycle.
*   **Decoupled:** They don't know about the UI (Views). They provide data or functionality that ViewModels can consume.
*   **Injected:** Services are managed by the [IServiceCollection](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-usage) container and are injected into other components via constructors.

## Key Characteristics

### 1. Lifecycle Management
Services can inherit from `AsyncDisposableOnce`. 
This allows them to perform asynchronous cleanup when the application shuts down, such as closing database connections or saving state.

### 2. Global State
Services are the perfect place to store state that needs to persist as the user navigates between pages. For example, 
the `NavigationService` keeps track of the navigation history, and the `ThemeService` remembers whether the user prefers Dark or Light mode.

### 3. Configuration
Some services need settings. By inheriting from `ServiceWithConfigBase<TConfig>`, 
a service automatically gains the ability to load and save its state using the framework's configuration system. 
See our [`LocalizationService`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Localization/LocalizationService.cs) for the example.

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

A typical service consists of an interface and an implementation:

```C#
// 1. Define the interface
public interface IMyCustomService
{
    void DoSomething();
}

// 2. Implement the service
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

    public MyViewModel(IMyCustomService service, ILoggerFactory loggerFactory)
        : base("view-model-id", loggerFactory)
    {
        _service = service;
        _service.DoSomething();
    }
}
```

## Service Registration

Services are registered explicitly in the builder chain using the standard .NET
[IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=net-10.0-pp) API.
This can be done anywhere that has access to the builder — directly in `Program.cs` or, more commonly,
inside a mixin extension method for your module (see [Registration via Mixin](#registration-via-mixin-recommended-for-modules) below).

### Standard Registration

The full list of registration methods and their semantics is covered in the [official docs](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=net-10.0-pp); below are the lifetimes used in this framework:

| Method                                    | Lifetime                                | Typical use in this framework                        |
|-------------------------------------------|-----------------------------------------|------------------------------------------------------|
| `AddSingleton<TService, TImpl>()`         | One instance for the whole app          | Services (navigation, theme, commands, …)            |
| `AddTransient<TService, TImpl>()`         | New instance every time it is requested | Extensions (`IExtensionFor<T>`), custom dialogs      |
| `AddKeyedTransient<TService, TImpl>(key)` | New instance per key per request        | Views (ViewLocator), keyed extensions                |

```C#
// Most services are singletons:
builder.Services.AddSingleton<IMyCustomService, MyCustomService>();

// Extensions and dialogs are transient:
builder.Services.AddTransient<IExtensionFor<IHomePage>, MyHomePageExtension>();
```

### Registration via Mixin (Recommended for Modules)

For better organization, create an extension method (mixin) for your module:

```C#
public static class MyModuleMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseMyModule()
        {
            builder.Services.AddSingleton<IMyCustomService, MyCustomService>();
            return builder;
        }
    }
}
```

Then use it in `Program.cs`:

```C#
builder.UseMyModule();
```

### Conditional Registration

Sometimes a service should only be registered under specific conditions.
Simply use standard `if` checks when registering:

```C#
public static class MyModuleMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseMyModule(Action<MyModuleOptions>? configure = null)
        {
            var options = new MyModuleOptions();
            configure?.Invoke(options);

            if (options.IsFeatureEnabled)
            {
                builder.Services.AddSingleton<IMyCustomService, MyCustomService>();
            }

            return builder;
        }
    }
}
```
