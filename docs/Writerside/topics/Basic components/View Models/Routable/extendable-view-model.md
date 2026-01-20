# Extendable View Model

## Overview

[`ExtendableViewModel<TSelfInterface>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Extendable/ExtendableViewModel.cs)
is a generic abstract class that extends [`RoutableViewModel`](routable-view-model.md) and adds support for dynamic extensibility.
It allows your view model to be extended with additional features.

## How It Works

1. MEF2 discovers your view model and looks for implementations of `IExtensionFor<TSelfInterface>`.
2. Each extension is instantiated and added to the `Extensions` collection.
3. The `Init` method is called automatically. It invokes each extension's `Extend` method, passing your view model and
   its disposable container.
4. `AfterLoadExtensions` is called to allow for any final setup in your view model.

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

### Extensions Collection

The Extensions property contains all imported extensions for your view model:

```C#
[ImportMany]
public IEnumerable<Lazy<IExtensionFor<TSelfInterface>>>? Extensions { get; set; }
```

These are automatically discovered and imported by MEF2 at startup.

### Initialization and Loading

The `Init` method is called automatically by MEF2 when all imports have been satisfied:

```C#
[OnImportsSatisfied]
public void Init()
{
    // Extensions are loaded and applied here
}
```

It iterates through extensions and calls the `Extend` method with your view model (returned by `GetContext`) and the
view model's `CompositeDisposable`.

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
[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
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

### [ExtendableViewModel&lt;TSelfInterface&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Extendable/ExtendableViewModel.cs)

Represents a base class for a view model that supports extensibility using MEF2.
This class provides a mechanism to load and apply extensions dynamically.

| Property     | Type                                                | Description                                                                                                                          |
|--------------|-----------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------|
| `Extensions` | `IEnumerable<Lazy<IExtensionFor<TSelfInterface>>>?` | Gets or sets a collection of extensions that enhance the functionality of the view model. Extensions are lazily imported using MEF2. |

| Method                  | Return Type      | Description                                                                                                                                               |
|-------------------------|------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Init()`                | `void`           | Called when MEF2 has completed importing dependencies. This method initializes and applies all available extensions to the current instance.              |
| `GetContext()`          | `TSelfInterface` | Gets the current instance as `TSelfInterface` or throws an exception if not implemented.                                                                  |
| `AfterLoadExtensions()` | `void`           | Called after all extensions have been loaded and applied. Derived classes must implement this method to provide additional logic after extension loading. |

### [IExtensionFor&lt;T&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Extendable/IExtensionFor.cs)

Defines a contract for an extension that can be applied to a specific type `T`.
This interface allows modular enhancements to be dynamically applied to existing objects.

| Method                                                  | Return Type | Description                                       |
|---------------------------------------------------------|-------------|---------------------------------------------------|
| `Extend(T context, CompositeDisposable contextDispose)` | `void`      | Applies the extension logic to the given context. |

#### `IExtensionFor<T>.Extend`

| Parameter        | Type                  | Description                                        |
|------------------|-----------------------|----------------------------------------------------|
| `context`        | `T`                   | The target object to extend.                       |
| `contextDispose` | `CompositeDisposable` | Disposable collection, that disposed with context. |

### [ExportExtensionForAttribute&lt;T&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Extendable/IExtensionFor.cs)

Marks a class as an exported extension for a specific type `T`.
This attribute is used with the Managed Extensibility Framework (MEF2) to enable automatic
discovery and composition of extensions.
