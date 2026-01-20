# Routable View Model

## Overview

[`RoutableViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/RoutableViewModel.cs)
is the core abstract class for all view models that need to support navigation and hierarchical structure in the
Asv.Avalonia framework. It extends [`DisposableViewModel`](disposable-view-model.md) and implements the
[`IRoutable`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/IRoutable.cs)
interface, providing:

- **Hierarchical Structure**: Creates a tree of view models with parent-child relationships, providing the structure
  and data required for navigation services.
- **Routed Events**: Propagates events through the hierarchy using different routing strategies.

Most view models you create should inherit from `RoutableViewModel` (directly or indirectly).

## Core Components

### Hierarchical Structure and Navigation Support

The `Parent` property holds a reference to the parent `IRoutable` in the navigation hierarchy. This creates a tree
structure where:

- Every view model (except the root) has exactly one parent.
- A view model can have multiple children.
- The hierarchy forms the basis for navigation and event propagation.

When adding child view models to a collection, use the [`SetRoutableParent`](#helper-methods-routablemixin) extension
methods to automatically manage parent references.

The `GetChildren` method must return an enumerable of all direct child view models. 
You must implement it in your view model:

```C#
public override IEnumerable<IRoutable> GetChildren()
{
    // Return all child view models
    foreach (var menu in MyMenuItems)
    {
        yield return menu;
    }
}
```

If your view model has no children, return an empty collection:

```C#
public override IEnumerable<IRoutable> GetChildren()
{
    return [];
}
```

The `Navigate` method is used by navigation services to find child view models by their ID. The default implementation
searches immediate children and returns the first match, or returns itself if no match is found.

> The actual navigation between view models is performed by the `INavigationService`, not by the `RoutableViewModel`
> itself.
> The view model just provides the structure and data needed for navigation to work.
> {style="note"}

### Routed Events

Routed events allow communication between view models in the hierarchy.
Instead of direct dependencies, view models can raise events that propagate through the hierarchy based on a routing
strategy.

#### Raising Events

Use the `Rise` method to raise an asynchronous routed event:

```C#
await myRoutableViewModel.Events.Rise(new MyCustomEvent(this, RoutingStrategy.Bubble));
```

Or with the helper method:
```C#
await myRoutableViewModel.Rise(new MyCustomEvent(this, RoutingStrategy.Bubble));
```

The event object must extend `AsyncRoutedEvent` from the Asv.Common library, which requires a source and a routing strategy.

#### Handling Events

To handle routed events, you can create an `InternalCatchEvent` method:

```C#
private ValueTask InternalCatchEvent(AsyncRoutedEvent e)
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

And subscribe to the `Events` property in the constructor:

```C#
Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
```

#### Routing Strategies

The event propagation is controlled by the `RoutingStrategy` enum:

- **Bubble**: The event starts at the source and propagates upward through parents to the root. Each parent view model
  can handle it or pass it further up.
- **Tunnel**: The event starts from the root and propagates downward through all children. Useful for distributing
  state changes or notifications from the top level.
- **Direct**: The event is only handled at the source and does not propagate through the hierarchy.

## Helper Methods (RoutableMixin)

The [`RoutableMixin`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/RoutableMixin.cs) class provides useful extension methods for working with `IRoutable` view models. For example:

### Setting Parent for Collections

Automatically set the parent for all items in a collection:

```C#
// For observable collections
var items = new ObservableList<MyViewModel>();
items.SetRoutableParent(this, Disposable);
```

The methods handle both existing items and items added/removed later. When items are removed, they are automatically
disposed.

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

```C#
public class ParentPageViewModel : PageViewModel<ParentPageViewModel>
{
    private readonly ObservableList<ItemViewModel> _items = [];

    // ...

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}

public class ItemViewModel : RoutableViewModel
{
    // ...
}
```

### Propagating Events Through the Hierarchy

Use routed events to communicate between view models without creating direct dependencies. For example, a child view
model can notify its parent when something needs attention:

```C#
// Define a custom routed event
public class ItemDeleteRequestedEvent(ItemViewModel source) 
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public ItemViewModel Item => source;
}

// In the ItemViewModel, request deletion when needed
public class ItemViewModel : RoutableViewModel
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
    
    public HelloWorldPageViewModel(
        ICommandService cmd, 
        ILoggerFactory loggerFactory, 
        IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
    {
        // ...
        
        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }
    
    // ...

    private ValueTask InternalCatchEvent(AsyncRoutedEvent e)
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

## Navigation with Navigation Service

`RoutableViewModel` provides the hierarchical structure that the `NavigationService` uses to navigate through your
application.

To ensure proper navigation, always:

- Set correct parent references using `SetRoutableParent`
- Return all children from `GetChildren()`
- Implement `Navigate` to correctly find children by their unique ID

## API {collapsible="true" default-state="collapsed"}

### [IRoutable](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/IRoutable.cs)

Represents a routable view model that supports navigation, hierarchical structure, and event propagation.
This interface extends `IViewModel` to include routing-related functionalities.

| Method                      | Return Type            | Description                                                                                                               |
|-----------------------------|------------------------|---------------------------------------------------------------------------------------------------------------------------|
| `Navigate(NavigationId id)` | `ValueTask<IRoutable>` | Navigates to a child routable element based on its identifier. This method is used to locate and return a routable child. |

#### `IRoutable.Navigate`

| Parameter | Type           | Description                                   |
|-----------|----------------|-----------------------------------------------|
| `id`      | `NavigationId` | The unique identifier of the target routable. |

### [RoutableViewModel: IRoutable](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/RoutableViewModel.cs)

| Property | Type                                | Description                                                             |
|----------|-------------------------------------|-------------------------------------------------------------------------|
| `Events` | `IRoutedEventController<IRoutable>` | Gets the controller for routed events within the `IRoutable` hierarchy. |
| `Parent` | `IRoutable?`                        | Gets or sets the parent `IRoutable` in the hierarchy.                   |

| Method          | Return Type              | Description                                                 |
|-----------------|--------------------------|-------------------------------------------------------------|
| `GetChildren()` | `IEnumerable<IRoutable>` | Returns all child `IRoutable` view models in the hierarchy. |
