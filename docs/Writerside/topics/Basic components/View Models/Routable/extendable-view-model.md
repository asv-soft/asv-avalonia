# Extendable View Model

## Overview

[`ExtendableViewModel<TSelfInterface>`](#extendableviewmodel-tselfinterface)
is a generic abstract class that extends [`RoutableViewModel`](routable-view-model.md) and adds support for dynamic extensibility.
It allows your view model to be extended with additional features.

## How It Works

1. The [`IExtensionService`](extension-service.md) discovers registered implementations of `IExtensionFor<TSelfInterface>` from the DI container.
2. Each extension's `Extend` method is invoked, passing your view model and its disposable container.
3. `AfterLoadExtensions` is called to allow for any final setup in your view model.

Extensions are loaded on the UI thread (via `Dispatcher.UIThread.Post`) to avoid deadlocks.

## Core Components

### Generic Parameter

The `TSelfInterface` generic parameter specifies the interface that your view model implements (or the view model type
itself).

### IExtensionFor Interface

Your extensions should implement the `IExtensionFor<in T>` interface, which contains a single method:

```C#
void Extend(T context, CompositeDisposable contextDispose);
```

This method is called when the extended view model is initialized.

- `context`: Represents the view model itself.
- `contextDispose`: The CompositeDisposable container of the view model.

You can register your `IDisposable` objects with it. Additionally, if your extension class implements `IDisposable`, the
view model will automatically register it to the `CompositeDisposable`.

### Extension Registration

Each extension is registered through the `Extensions` builder available on the application host builder.
Registration can happen in any place that has access to the builder — `Program.cs`, a dedicated module, a plugin entry point, etc.

```C#
builder.Extensions.Register<IHomePage, MyHomePageExtension>();
```

This registers `MyHomePageExtension` as a transient service implementing `IExtensionFor<IHomePage>` in the `IServiceCollection`.
At runtime the DI container resolves all registered `IExtensionFor<IHomePage>` implementations and applies them to every `IHomePage` instance.

### Initialization and Loading

Extensions are loaded automatically in the constructor of `ExtendableViewModel` via the `IExtensionService`.
The service resolves all registered `IExtensionFor<TSelfInterface>` implementations and calls their `Extend` method.

You must implement the abstract `AfterLoadExtensions` method to perform any initialization required after extensions are
loaded:

```C#
protected abstract void AfterLoadExtensions();
```

You can leave this method empty if no post-initialization logic is needed.

## Example

A basic usage example is our Home Page. When adding pages to an app using Asv.Avalonia, you will likely create an
Extension for `IHomePage` so your page is accessible from the home tools menu.

These extensions look like this:

```C#
public sealed class HomePageLogViewerExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, R3.CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenLogViewerCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                    RS.OpenLogViewerCommand_Action_Title,
                    RS.OpenLogViewerCommand_Action_Description
                )
                .DisposeItWith(contextDispose)
        );
    }
}
```

In this example, we add a new tool to the home page that opens the LogViewer page.
We export an extension for the `IHomePage` interface, which is implemented by `HomePageViewModel`.
The interface contains the `Tools` collection, which we modify.

```C#
public class HomePageViewModel : PageViewModel<IHomePage>, IHomePage 
{
    // ...
    
    public ObservableList<IActionViewModel> Tools { get; }
    
    // ...
}
```

## API {collapsible="true" default-state="collapsed"}

### [ExtendableViewModel&lt;TSelfInterface&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Extendable/ExtendableViewModel.cs)

Represents a base class for a view model that supports extensibility via the [`IExtensionService`](extension-service.md).
This class provides a mechanism to load and apply extensions dynamically.

| Method                  | Return Type      | Description                                                                                                                                               |
|-------------------------|------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GetContext()`          | `TSelfInterface` | Gets the current instance as `TSelfInterface` or throws an exception if not implemented.                                                                  |
| `AfterLoadExtensions()` | `void`           | Called after all extensions have been loaded and applied. Derived classes must implement this method to provide additional logic after extension loading. |
