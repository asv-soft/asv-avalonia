# View Model

## Overview

[`ViewModel`](#viewmodel-iviewmodel) is the base abstract class for all view models in the Asv.Avalonia framework.
It implements the [`IViewModel`](#iviewmodel) interface and provides core features for property notification,
resource management, hierarchical structure and routed events.

Key Features:

- **Property Change Notifications**: implements [`INotifyPropertyChanged`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-10.0)
  and [`INotifyPropertyChanging`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging?view=net-10.0).
- **Resource Management**: a `CancellationToken` and disposable resource collections, with a thread-safe disposal
  pattern ensuring resources are released exactly once.
- **Hierarchical Structure**: creates a tree of view models with parent-child relationships, providing the structure
  and data required for navigation.
- **Routed Events**: propagates events through the hierarchy using different routing strategies.

Every view model inherits from `ViewModel`, or from [`ViewModel<TExtensionIfc>`](extendable-view-model.md) when it
must be open to extensions.

## Core Components

### NavId

The `Id` property (of type `NavId`) identifies the view model instance. It's essential for:

- Identifying: Distinguishing ViewModels within the application.
- Navigation: Providing the type id and parameters for routing via `Navigate` and the
  [helper methods](#helper-methods-routablemixin) in `RoutableMixin`.

You supply custom arguments through the constructor — `ViewModel(string typeId, NavArgs args = default)` builds the
`NavId` from the type id and the arguments. `Id` is immutable after construction.

`NavId` does not enforce uniqueness by itself. IDs must be unique within their runtime context; this requirement is
validated at runtime.

### Property Notification

`IViewModel` inherits from [`INotifyPropertyChanged`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-10.0)
and [`INotifyPropertyChanging`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging?view=net-10.0).

To update properties, use the `SetField` method. It updates the backing field and automatically raises the
`PropertyChanging` and `PropertyChanged` events only when the value has actually changed, preventing unnecessary UI
updates.

### Disposable Resource Collections

`ViewModel` provides two protected collections for resources that must be disposed together with the view model:

- `DisposableBag` is an add-only value type with lower allocation overhead. Register resources with
  `.AddTo(ref DisposableBag)`. Prefer it when resources are added from a single thread and do not need to be removed
  individually.
- `Disposable` is a thread-safe `CompositeDisposable` that supports removing individual resources. Register resources
  with `.DisposeItWith(Disposable)`. Prefer it when resources can be registered concurrently or must be removed before
  the view model is disposed.

Both collections automatically dispose all remaining registered resources when the view model is disposed.

After the view model is disposed, `Disposable` returns a shared, already-disposed container: registering a resource on
it disposes that resource immediately instead of leaking it, so late registrations from racing background work are
safe.

### CancellationToken

The `DisposeCancel` property provides a `CancellationToken` that is automatically triggered when the ViewModel starts
its disposal process.

- **Thread-Safety**: The cancellation and disposal of the source are handled in a thread-safe manner.
- **Primary Use Case**: Passing the token to asynchronous tasks, background loops, or observable subscriptions to ensure
  they stop immediately when the ViewModel is destroyed.

### Disposal Pattern

`ViewModel` implements a thread-safe disposal mechanism:

1. The public `Dispose()` method handles thread safety and prevents multiple disposal calls.
2. Derived classes should override the `Dispose(bool disposing)` method to perform their own cleanup (e.g.
   unsubscribing from events or stopping timers), and call `base.Dispose(disposing)`.

The base implementation tears down in a specific order:

1. Detach: `Parent` is set to `null` and the `PropertyChanging` / `PropertyChanged` handlers are cleared.
2. Cancel: The `CancellationToken` is canceled, notifying all linked tasks to stop.
3. Dispose Source: The `CancellationTokenSource` itself is disposed.
4. Dispose Resources: The `Disposable` container is disposed, which in turn disposes of all registered objects.
5. Dispose Bag: The `DisposableBag` is disposed.

### Hierarchical Structure and Navigation Support

The `Parent` property holds a reference to the parent `IViewModel` in the navigation hierarchy. This creates a tree
structure where:

- Every view model (except the root) has exactly one parent.
- A view model can have multiple children.
- The hierarchy forms the basis for navigation and event propagation.

`Parent` is read-only — assign it with the `SetParent` method. When adding child view models to a collection, use the
[`SetRoutableParent`](#helper-methods-routablemixin) extension methods to manage parent references automatically.

The `GetChildren` method returns an enumerable of all direct child view models. It has a default implementation
returning an empty collection, so you only need to override it when your view model has children:

```C#
public override IEnumerable<IViewModel> GetChildren()
{
    // Return all child view models
    foreach (var menu in MyMenuItems)
    {
        yield return menu;
    }
}
```

The `Navigate` method is used to find child view models by their ID. The default implementation searches immediate
children and returns the first match, or returns itself if no match is found.

### Routed Events

Routed events allow communication between view models in the hierarchy.
Instead of direct dependencies, view models can raise events that propagate through the hierarchy based on a routing
strategy.

#### Raising Events

Use the `Rise` method to raise an asynchronous routed event:

```C#
await myViewModel.Events.Rise(new MyCustomEvent(this, RoutingStrategy.Bubble));
```

Or with the helper method:

```C#
await myViewModel.Rise(new MyCustomEvent(this, RoutingStrategy.Bubble));
```

The event object must extend `AsyncRoutedEvent` from the Asv.Modeling library, which requires a source and a routing
strategy.

#### Handling Events

To handle routed events, you can create an `InternalCatchEvent` method:

```C#
private ValueTask InternalCatchEvent(
    IViewModel src,
    AsyncRoutedEvent<IViewModel> e,
    CancellationToken cancel
)
{
    switch (e)
    {
        case MyCustomEvent customEvent:
            e.IsHandled = true; // stop further propagation
            break;
    }

    return ValueTask.CompletedTask;
}
```

And catch events via the `Events` property in the constructor:

```C#
Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
```

There is also a typed overload that filters by event type for you:

```C#
Events.Catch<MyCustomEvent>(OnMyCustomEvent).DisposeItWith(Disposable);
```

#### Routing Strategies

The event propagation is controlled by the `RoutingStrategy` enum:

- **Bubble**: The event starts at the source and propagates upward through parents to the root. Each parent view model
  can handle it or pass it further up.
- **Tunnel**: The event starts at the view model it is raised on and propagates downward through its children. Raise it
  on the root (see [Finding the Root](#finding-the-root)) to reach the whole tree. Useful for distributing state changes
  or notifications from the top level.
- **Direct**: The event is only handled at the source and does not propagate through the hierarchy.

## Helper Methods (RoutableMixin)

The [`RoutableMixin`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Routable/RoutableMixin.cs) class provides useful extension methods for working with `IViewModel` view models. For example:

### Setting Parent for Collections

Automatically set the parent for all items in a collection:

```C#
// For observable collections
var items = new ObservableList<MyViewModel>();
items.SetRoutableParent(this).DisposeItWith(Disposable);
```

The methods handle both existing items and items added/removed later. Removed items have their parent reset to `null`.
The overloads for collections implementing `INotifyCollectionChanged` additionally dispose removed items.

### Navigating by Path

Navigate through the hierarchy using a complete path:

```C#
var target = await myViewModel.NavigateByPath(navigationPath);
```

### Finding the Root

Get the topmost parent in the hierarchy:

```C#
var root = myViewModel.GetRoot();
```

## Common Use Cases

### Managing Child Elements

A common pattern is to have a parent view model that contains child view models. These child elements are automatically
part of the hierarchy and can be navigated to individually. For example:

The constructor and unrelated members are omitted. The constructor must register `_items` with
`SetRoutableParent(this)` to maintain parent references.

```C#
public class ParentPageViewModel : PageViewModel<ParentPageViewModel>
{
    private readonly ObservableList<ItemViewModel> _items = [];

    // ...

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}

public class ItemViewModel : ViewModel
{
    public ItemViewModel(string typeId)
        : base(typeId) { }

    // ...
}
```

### Propagating Events Through the Hierarchy

Use routed events to communicate between view models without creating direct dependencies. For example, a child view
model can notify its parent when something needs attention:

```C#
// Define a custom routed event
public class ItemDeleteRequestedEvent(ItemViewModel source) 
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public ItemViewModel Item => source;
}

// In the ItemViewModel, request deletion when needed
public class ItemViewModel : ViewModel
{
    // ...
    
    public async Task Delete()
    {
        // Notify parent that this item wants to be deleted
        await this.Rise(new ItemDeleteRequestedEvent(this));
    }
}

// In the parent, handle the deletion request
public class ParentPageViewModel : PageViewModel<ParentPageViewModel>
{
    // ...
    
    private readonly ObservableList<ItemViewModel> _items = [];
    
    public ParentPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext) : base(PageId, context, loggerFactory, dialogService, ext)
    {
        // ...

        _items.SetRoutableParent(this).AddTo(ref DisposableBag);
        _items.DisposeRemovedItems().AddTo(ref DisposableBag);
        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
    }
    
    // ...

    private ValueTask InternalCatchEvent(
        IViewModel src,
        AsyncRoutedEvent<IViewModel> e,
        CancellationToken cancel
    )
    {
        if (e is ItemDeleteRequestedEvent deleteEvent)
        {
            _items.Remove(deleteEvent.Item);
            e.IsHandled = true;
        }
        
        return ValueTask.CompletedTask;
    }
}
```

The parent handles the event and removes the item from its collection. This way, the child doesn't need a direct
reference to the parent — it just raises an event and the parent decides what to do.

## API {collapsible="true" default-state="collapsed"}

### [IViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/IViewModel.cs)

Defines a base contract for all view models in the application. It declares no members of its own — it composes
`IDisposable`, `INotifyPropertyChanging`, `INotifyPropertyChanged` and the Asv.Modeling `ISupportUndo<IViewModel>`,
`ISupportParentChange<IViewModel>`, `ISupportRootTracking<IViewModel, IShell>` and `ISupportLayout` interfaces, which
supply identity, hierarchy, routed events, undo and layout support.

### [ViewModel: IViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/ViewModel.cs)

Represents the base implementation of a view model that provides
property change notifications and a proper undo and disposal mechanism.
This class is designed to be inherited by other view models.

#### `ViewModel` constructor

| Parameter | Type      | Description                                                                  |
|-----------|-----------|------------------------------------------------------------------------------|
| `typeId`  | `string`  | The type identifier the `Id` is built from.                                  |
| `args`    | `NavArgs` | Optional navigation arguments. Defaults to `default`.                        |

| Property        | Type                                 | Description                                                                                                                                                                          |
|-----------------|--------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Id`            | `NavId`                              | Identifies the ViewModel within its runtime context. `NavId` itself does not enforce uniqueness.                                                                                     |
| `Tag`           | `object?`                            | Gets or sets an arbitrary user-defined object associated with the view model.                                                                                                        |
| `Parent`        | `IViewModel?`                        | Gets the parent `IViewModel` in the hierarchy. Set it via `SetParent`.                                                                                                               |
| `ParentChanged` | `Observable<IViewModel?>`            | Gets an observable that emits the new parent every time `SetParent` is called.                                                                                                       |
| `Events`        | `IRoutedEventController<IViewModel>` | Gets the controller for routed events within the `IViewModel` hierarchy.                                                                                                             |
| `RootTracking`  | `IRootTrackingController<IShell>`    | Gets the controller that tracks the `IShell` root this view model is attached to.                                                                                                    |
| `Undo`          | `IUndoController`                    | Gets the undo controller of the view model, used to register undoable values via `Undo.Register` and `Undo.RegisterValue`. Created on first access and disposed with the view model. |
| `Layout`        | `ILayoutController`                  | Gets the layout controller of the view model, used to register values that are saved and restored via `Layout.Register`. Created on first access and disposed with the view model.   |
| `IsDisposed`    | `bool`                               | Gets a value indicating whether the view model has been disposed.                                                                                                                    |
| `DisposeCancel` | `CancellationToken`                  | Protected. A cancellation token linked to the disposal state of the view model. If the view model is disposed, the token is set to `CancellationToken.None`.                         |
| `Disposable`    | `CompositeDisposable`                | Protected. A thread-safe collection for resources registered via `DisposeItWith(Disposable)`. Disposed together with the view model.                                                 |
| `DisposableBag` | `ref DisposableBag`                  | Protected. An add-only collection for resources registered via `AddTo(ref DisposableBag)`. Disposed together with the view model.                                                    |

| Event              | Type                            | Description                                                                                              |
|--------------------|---------------------------------|----------------------------------------------------------------------------------------------------------|
| `PropertyChanged`  | `PropertyChangedEventHandler?`  | Occurs when a property value changes. Implements `INotifyPropertyChanged` to support UI binding updates. |
| `PropertyChanging` | `PropertyChangingEventHandler?` | Occurs when a property value is about to change.                                                         |

| Method                                                    | Return Type               | Description                                                                                                         |
|-----------------------------------------------------------|---------------------------|---------------------------------------------------------------------------------------------------------------------|
| `Dispose()`                                               | `void`                    | Releases resources used by the view model. Ensures that the disposal operation is only performed once.              |
| `Dispose(bool disposing)`                                 | `void`                    | Protected virtual. Releases managed resources when disposing. Derived classes override it to add their own cleanup. |
| `SetParent(IViewModel? parent)`                           | `void`                    | Sets the parent and raises `ParentChanged`.                                                                         |
| `GetChildren()`                                           | `IEnumerable<IViewModel>` | Virtual. Returns the direct child view models. Returns an empty collection by default.                              |
| `Navigate(NavId id)`                                      | `ValueTask<IViewModel>`   | Virtual. Finds a direct child by its identifier; returns itself if no child matches.                                |
| `ToString()`                                              | `string`                  | Override. Returns a string representation of the view model in the form `TypeName[Id]`.                             |
| `SetField<T>(ref T field, T value, string? propertyName)` | `bool`                    | Protected. Sets the field to the specified value and raises the change events if the value has changed.             |

#### `ViewModel.Dispose(bool disposing)`

| Parameter   | Type   | Description                                                                                                           |
|-------------|--------|-----------------------------------------------------------------------------------------------------------------------|
| `disposing` | `bool` | `true` if called from `Dispose` method to release managed resources; otherwise, `false` if called from the finalizer. |

#### `ViewModel.SetField<T>`

| Parameter      | Type      | Description                                                                             |
|----------------|-----------|-----------------------------------------------------------------------------------------|
| `field`        | `ref T`   | The backing field reference.                                                            |
| `value`        | `T`       | The new value to set.                                                                   |
| `propertyName` | `string?` | The name of the property that changed. Automatically set by the caller if not provided. |
