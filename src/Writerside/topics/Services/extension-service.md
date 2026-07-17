# Extension Service

## Overview

`IExtensionService` is the mechanism that lets any view model be augmented with additional behavior without modifying
its source code. A view model becomes extendable by deriving from the generic base `ViewModel<TSelfInterface>`, where
`TSelfInterface` is the interface it exposes to extensions.

When such a view model is initialized, its `ViewModel<TSelfInterface>` base asks `IExtensionService` to resolve every
`IExtensionFor<TSelfInterface>` registered for its type and applies each one.
This keeps the view model itself clean while plugins and modules can freely add commands, tools, or other logic to it.

## Registration

Register extensions through the `Extensions` builder. A non-keyed extension is applied to every
extended object that exposes the specified context interface:

```C#
builder.Extensions
    .Register<IHomePage, HomePageSearchExtension>()
    .Register<IHomePage, HomePageSettingsExtension>();
```

Use the overload that accepts a key when an extension should apply only to objects whose `Id.TypeId`
matches that key:

```C#
builder.Extensions.Register<IHomePage, DeviceHomePageExtension>("device-home");
```

Extensions and policies are registered as transient services. The extension service resolves the
non-keyed registrations first, followed by the registrations keyed with the target object's type id.

## Extension Policies

An `IExtensionPolicyFor<TContext>` can filter, replace or reorder the resolved extensions before they
are applied. Policies run in ascending `Order`; each policy receives the result of the preceding one.
For example, this policy removes duplicate extension ids:

```C#
public sealed class DistinctHomePageExtensionsPolicy : IExtensionPolicyFor<IHomePage>
{
    public const string StaticId = "policy.home.distinct";

    public string Id => StaticId;
    public int Order => 0;

    public IEnumerable<IExtensionFor<IHomePage>> Filter(
        IHomePage context,
        IEnumerable<IExtensionFor<IHomePage>> extensions
    )
    {
        return extensions.DistinctBy(extension => extension.Id);
    }
}
```

Register a policy for all objects exposing the context interface, or pass a key to limit it to a
specific owner type id:

```C#
// For every IHomePage owner:
builder.Extensions.RegisterPolicy<IHomePage, DistinctHomePageExtensionsPolicy>();

// Or only when the owner's type id is "device-home":
builder.Extensions.RegisterPolicy<IHomePage, DistinctHomePageExtensionsPolicy>("device-home");
```

When a policy removes an extension that implements `IDisposable`, the service disposes that extension
immediately. Extensions that remain and implement `IDisposable` are tied to the owner's disposable
collection.

## API {collapsible="true" default-state="collapsed"}

### [IExtensionService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Extensions/IExtensionService.cs)

Resolves and applies all registered `IExtensionFor<TInterface>` extensions to a given owner.
Used internally by the `ViewModel<TSelfInterface>` base to load extensions from the DI container.

| Method                                                                                          | Return Type | Description                                                             |
|-------------------------------------------------------------------------------------------------|-------------|-------------------------------------------------------------------------|
| `Extend<TInterface>(TInterface owner, string ownerTypeId, CompositeDisposable ownerDisposable)` | `void`      | Resolves all extensions for `TInterface` and applies them to the owner. |

#### `IExtensionService.Extend`

| Parameter          | Type                  | Description                                                                                                       |
|--------------------|-----------------------|-------------------------------------------------------------------------------------------------------------------|
| `owner`            | `TInterface`          | The target object to extend.                                                                                      |
| `ownerTypeId`      | `string`              | Type id used as the key to resolve keyed extensions from the DI container in addition to non-keyed ones.          |
| `ownerDisposable`  | `CompositeDisposable` | Disposable collection tied to the owner's lifetime. Extensions and their disposables are registered here.         |

### [IExtensionFor&lt;T&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Extensions/IExtensionFor.cs)

Defines a contract for an extension that can be applied to a specific type `T`.
This interface allows modular enhancements to be dynamically applied to existing objects.

| Method                                                  | Return Type | Description                                       |
|---------------------------------------------------------|-------------|---------------------------------------------------|
| `Extend(T context, CompositeDisposable contextDispose)` | `void`      | Applies the extension logic to the given context. |

#### `IExtensionFor<T>.Extend`

| Parameter        | Type                  | Description                                              |
|------------------|-----------------------|----------------------------------------------------------|
| `context`        | `T`                   | The target object to extend.                             |
| `contextDispose` | `CompositeDisposable` | Disposable collection that is disposed with the context. |

### [IExtensionPolicyFor&lt;T&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Extensions/IExtensionPolicy.cs)

Defines a policy that can inspect and transform the set of extensions before they are applied to a context.
Policies are evaluated by `IExtensionService` before the extensions are applied.

| Property | Type     | Description                              |
|----------|----------|------------------------------------------|
| `Id`     | `string` | Unique policy id used in diagnostics.    |
| `Order`  | `int`    | Execution order; lower values run first. |

| Method                                                        | Return Type                       | Description                                                       |
|---------------------------------------------------------------|-----------------------------------|-------------------------------------------------------------------|
| `Filter(T context, IEnumerable<IExtensionFor<T>> extensions)` | `IEnumerable<IExtensionFor<T>>`   | Filters, replaces or reorders extensions for the specified owner. |

#### `IExtensionPolicyFor<T>.Filter`

| Parameter    | Type                            | Description                                         |
|--------------|---------------------------------|-----------------------------------------------------|
| `context`    | `T`                             | The target object that will receive the extensions. |
| `extensions` | `IEnumerable<IExtensionFor<T>>` | The extensions resolved for the target object.      |

### [ExtensionsRegistrations.Builder](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Extensions/ExtensionsRegistrations.cs)

Registers extensions and extension policies.

| Method                                          | Return Type | Description                                                     |
|-------------------------------------------------|-------------|-----------------------------------------------------------------|
| `Register<TContext, TExtension>()`              | `Builder`   | Registers an extension for every owner exposing `TContext`.     |
| `Register<TContext, TExtension>(string key)`    | `Builder`   | Registers an extension for owners whose type id matches `key`.  |
| `RegisterPolicy<TContext, TPolicy>()`           | `Builder`   | Registers a policy for every owner exposing `TContext`.         |
| `RegisterPolicy<TContext, TPolicy>(string key)` | `Builder`   | Registers a policy for owners whose type id matches `key`.      |

#### `ExtensionsRegistrations.Builder.Register<TContext, TExtension>(string key)`

| Parameter | Type     | Description                                                     |
|-----------|----------|-----------------------------------------------------------------|
| `key`     | `string` | The owner type identifier used as the dependency injection key. |

#### `ExtensionsRegistrations.Builder.RegisterPolicy<TContext, TPolicy>(string key)`

| Parameter | Type     | Description                                                     |
|-----------|----------|-----------------------------------------------------------------|
| `key`     | `string` | The owner type identifier used as the dependency injection key. |
