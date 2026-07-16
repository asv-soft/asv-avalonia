# Extendable View Model

## Overview

[`ViewModel<TExtensionIfc>`](#viewmodel-textensionifc) is a generic abstract class that extends
[`ViewModel`](view-model.md) and adds support for dynamic extensibility. It allows your view model to be extended with
additional features.

## How It Works

1.  The [`IExtensionService`](extension-service.md) discovers registered implementations of
   `IExtensionFor<TExtensionIfc>` from the DI container.
2. Each extension's `Extend` method is invoked, passing your view model and its disposable container.
3. `AfterLoadExtensions` is called to allow for any final setup in your view model.

Extensions are loaded on the UI thread (via `Dispatcher.UIThread.Post`) to avoid deadlocks.

## Core Components

### Generic Parameter

The `TExtensionIfc` generic parameter specifies the interface that your view model implements (or the view model type
itself). The constructor validates the cast once and throws if your class does not implement it.

### IExtensionFor Interface

Your extensions should implement the `IExtensionFor<in TContext>` interface, which derives from `ISupportId<string>`.
You must supply a stable `Id` — used for logging and extension policies — and implement `Extend`:

```C#
string Id { get; }

void Extend(TContext context, CompositeDisposable contextDispose);
```

`Extend` is called when the extended view model is initialized.

- `context`: Represents the view model itself.
- `contextDispose`: The CompositeDisposable container of the view model.

You can register your `IDisposable` objects with it. Additionally, if your extension class implements `IDisposable`, the
view model will automatically register it to the `CompositeDisposable`.

### Extension Registration

Each extension is registered through the `Extensions` builder available on the application host builder.
Registration can happen in any place that has access to the builder — `Program.cs`, a dedicated module, a plugin entry
point, etc.

```C#
builder.Extensions.Register<IHomePage, MyHomePageExtension>();
```

This registers `MyHomePageExtension` as a transient service implementing `IExtensionFor<IHomePage>` in the
`IServiceCollection`. At runtime the DI container resolves all registered `IExtensionFor<IHomePage>` implementations and
applies them to every `IHomePage` instance.

### Initialization and Loading

The constructor of `ViewModel<TExtensionIfc>` schedules extension loading via the `IExtensionService`. The work is
posted to the UI thread at `DispatcherPriority.Background`, so extensions are applied shortly *after* the constructor
returns — do not assume they are in place by the end of your own constructor.

The service resolves both non-keyed `IExtensionFor<TExtensionIfc>` registrations and keyed ones matching the view
model's `Id.TypeId`, then calls `Extend` on each.

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
public sealed class HomePageLogViewerExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.log-viewer";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-log-viewer")
        {
            Header = RS.OpenLogViewerCommand_Action_Title,
            Description = RS.OpenLogViewerCommand_Action_Description,
            Icon = LogViewerViewModel.PageIcon,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(LogViewerViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
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

### [ViewModel&lt;TExtensionIfc&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/ViewModel.cs)

Represents a base class for a view model that supports extensibility via the
[`IExtensionService`](extension-service.md). This class provides a mechanism to load and apply extensions dynamically.
Constrained to `where TExtensionIfc : class`.

#### `ViewModel<TExtensionIfc>` constructor

| Constructor                                                                                 | Description                                                                                                                        |
|---------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------|
| `ViewModel<TExtensionIfc>(string typeId, NavArgs args, IExtensionService extensionService)` | Protected. Creates the view model, checks that the class implements `TExtensionIfc`, and posts extension loading to the UI thread. |

| Property  | Type            | Description                                                                                                                                         |
|-----------|-----------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| `Context` | `TExtensionIfc` | Protected. The current instance cast to `TExtensionIfc`. The cast is validated in the constructor, which throws if the class does not implement it. |

| Method                  | Return Type      | Description                                                                                                                                               |
|-------------------------|------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `AfterLoadExtensions()` | `void`           | Called after all extensions have been loaded and applied. Derived classes must implement this method to provide additional logic after extension loading. |
