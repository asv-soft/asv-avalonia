# Extension Service

## Overview

`IExtensionService` is the mechanism that allows any `ExtendableViewModel` to be augmented with additional behavior without modifying its source code.

At startup, each extension is registered in the DI container via the `Extensions` builder:

```C#
builder.Extensions.Register<IHomePage, MyHomePageExtension>();
```

When an `ExtendableViewModel` is initialized, it asks `IExtensionService` to resolve every `IExtensionFor<TSelfInterface>` registered for its type and calls `Extend` on each one.
This keeps the view model itself clean while plugins and modules can freely add commands, tools, or other logic to it.

For a detailed walkthrough see [Extendable View Model](extendable-view-model.md).

## API {collapsible="true" default-state="collapsed"}

### [IExtensionService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/AppHost/Extensions/IExtensionService.cs)

Resolves and applies all registered `IExtensionFor<TInterface>` extensions to a given owner.
Used internally by `ExtendableViewModel` to load extensions from the DI container.

| Method                                                                                       | Return Type | Description                                                             |
|----------------------------------------------------------------------------------------------|-------------|-------------------------------------------------------------------------|
| `Extend<TInterface>(TInterface owner, string ownerKey, CompositeDisposable ownerDisposable)` | `void`      | Resolves all extensions for `TInterface` and applies them to the owner. |

#### `IExtensionService.Extend`

| Parameter          | Type                  | Description                                                                                                       |
|--------------------|-----------------------|-------------------------------------------------------------------------------------------------------------------|
| `owner`            | `TInterface`          | The target object to extend.                                                                                      |
| `ownerKey`         | `string`              | Key used to resolve keyed extensions from the DI container in addition to non-keyed ones.                         |
| `ownerDisposable`  | `CompositeDisposable` | Disposable collection tied to the owner's lifetime. Extensions and their disposables are registered here.         |

The default implementation (`ExtensionService`) resolves both non-keyed (`GetServices<IExtensionFor<TInterface>>()`) 
and keyed (`GetKeyedServices<IExtensionFor<TInterface>>(ownerKey)`) extensions from the `IServiceProvider`. 
If an extension implements `IDisposable`, it is automatically added to `ownerDisposable`.

### [IExtensionFor&lt;T&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/AppHost/Extensions/IExtensionFor.cs)

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

