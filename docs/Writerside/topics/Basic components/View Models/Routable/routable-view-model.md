# Routable View Model

## Overview

[
`RoutableViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/RoutableViewModel.cs)
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

The `GetRoutableChildren` method must return an enumerable of all direct child view models. 
You must implement it in your view model:

```C#
public override IEnumerable<IRoutable> GetRoutableChildren()
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
public override IEnumerable<IRoutable> GetRoutableChildren()
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
await myViewModel.Rise(new MyCustomEvent(this, RoutingStrategy.Bubble));
```

The event object must extend [
`AsyncRoutedEvent`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/IRoutable.cs),
which requires a source and a routing strategy.

#### Routing Strategies

The event propagation is controlled by the `RoutingStrategy` enum:

- **Bubble**: The event starts at the source and propagates upward through parents to the root. Each parent view model
  can handle it or pass it further up.
- **Tunnel**: The event starts from the root and propagates downward through all children. Useful for distributing
  state changes or notifications from the top level.
- **Direct**: The event is only handled at the source and does not propagate through the hierarchy.

#### Handling Events

To handle routed events, you can override `InternalCatchEvent`:

```C#
protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
{
    switch (e)
    {
        case MyCustomEvent customEvent:
            e.IsHandled = true;
            break;
    }
    return base.InternalCatchEvent(e);
}
```

When you set `IsHandled = true` on the event, further propagation stops.

## Helper Methods (RoutableMixin)

The [
`RoutableMixin`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Routable/RoutableMixin.cs)
class provides useful extension methods for working with `IRoutable` view models. For example:

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

### Hierarchy Traversal

Several methods help you traverse the hierarchy:

- **`GetAncestorsToRoot()`**: Get all parents from current to root
- **`GetHierarchyFromRoot()`**: Get all parents from root to current (reverse order)
- **`GetAncestorsTo(target)`**: Get all ancestors up to a specific target
- **`FindParentOfType<T>()`**: Find the first parent of a specific type
- **`GetPathToRoot()`**: Get the navigation path from current to root

## Common Use Cases

### Managing Child Elements

A common pattern is to have a parent view model that contains child view models. These child elements are automatically
part of the hierarchy and can be navigated to individually. For example:

```C#
public class ParentPageViewModel : PageViewModel<ParentPageViewModel>
{
    private readonly ObservableList<ItemViewModel> _items = [];

    // ...

    public override IEnumerable<IRoutable> GetRoutableChildren()
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
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
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
    private readonly ObservableList<ItemViewModel> _items = [];
    
    // ...

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ItemDeleteRequestedEvent deleteEvent)
        {
            _items.Remove(deleteEvent.Item);
            e.IsHandled = true;
        }
        return base.InternalCatchEvent(e);
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
- Return all children from `GetRoutableChildren()`
- Implement `Navigate` to correctly find children by their unique ID