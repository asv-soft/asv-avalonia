# Tree Subpage

## Overview

[`TreeSubpage`](#treesubpage-itreesubpage) is an abstract base class for content pages displayed within a [`TreePageViewModel`](tree-page-view-model.md).

It extends [`RoutableViewModel`](routable-view-model.md) and implements the [`ITreeSubpage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/ITreeSubpage.cs) interface.

TreeSubpage serves as the detail view that appears on the right side when a user selects a node in the tree menu. It includes its own menu system and can be extended through the DI container.

## Core Components

### Menu System

Each subpage has its own `Menu` collection and `MenuView` tree structure. This allows subpages to have their own toolbar or contextual menu items.

The menu items are automatically disposed when the subpage is disposed, and they inherit the routing parent from the subpage itself.

### Generic Context

`TreeSubpage<TContext>` is a generic variant that receives a context object during initialization via the `Init` method. This context is typically the parent page that hosts the subpage, allowing the subpage to access shared data or services.

## Example

A typical usage pattern involves creating a subpage interface, a base implementation, and then concrete subpages.

First, create an interface for your tree subpage (e.g., [`ISettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/ISettingsSubPage.cs)):

```C#
public interface ISettingsSubPage : ITreeSubpage<ISettingsPage> 
{ 
}
```

Next, implement a base class (e.g., [`SettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/SettingsSubPage.cs)):

```C#
public abstract class SettingsSubPage(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage<ISettingsPage>(id, loggerFactory),
        ISettingsSubPage
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;
}
```

Now create a concrete subpage view model. Simply inherit from the base class created above:

```C#
public class SettingsUnitsViewModel : SettingsSubPage
{
    public const string PageId = "units";

    public SettingsUnitsViewModel(ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        // Initialize your properties and commands here
    }

    public override IEnumerable<IRoutable> GetChildren() => [];
}
```

> If the subpage needs extensibility (e.g., allowing plugins to add sections), inherit from [`ExtendableTreeSubpage`](extendable-tree-subpage.md) instead.
> {style="note"}

Each subpage needs a tree menu node — this is what appears in the left-hand tree. Create a class that inherits from `TreePage`:

```C#
public class SettingsUnitTreePageMenu : TreePage
{
    public SettingsUnitTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsUnitsViewModel.PageId,   // node ID
            "Units",                         // display name
            MaterialIconKind.KeyboardSettings, // icon
            SettingsUnitsViewModel.PageId,   // navigation target (subpage ID)
            NavigationId.Empty,              // parent node (empty = root level)
            loggerFactory
        ) { }
}
```

Finally, register the subpage, its view, and the tree menu node:

```C#
// Register the subpage view model (keyed by subpage ID)
builder.Services.AddKeyedTransient<ISettingsSubPage, SettingsUnitsViewModel>(
    SettingsUnitsViewModel.PageId);

// Register the view
builder.ViewLocator.RegisterViewFor<SettingsUnitsViewModel, SettingsUnitsView>();

// Register the tree menu node (keyed by the parent page ID)
builder.Services.AddKeyedTransient<ITreePage, SettingsUnitTreePageMenu>(
    SettingsPageViewModel.PageId);
```

## API {collapsible="true" default-state="collapsed"}

### [ITreeSubpage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/ITreeSubpage.cs)

Represents a subpage that can be displayed in a tree-based page structure. 
Extends `IRoutable` to provide routing capabilities.

| Property   | Type                        | Description                                                 |
|------------|-----------------------------|-------------------------------------------------------------|
| `MenuView` | `MenuTree`                  | Gets the tree structure for the subpage's menu.             |
| `Menu`     | `ObservableList<IMenuItem>` | Gets the collection of menu items associated with the page. |

### [ITreeSubpage&lt;TContext&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/ITreeSubpage.cs)

Generic variant of `ITreeSubpage` that receives a context during initialization.

| Method                   | Return Type | Description                                                |
|--------------------------|-------------|------------------------------------------------------------|
| `Init(TContext context)` | `ValueTask` | Initializes the subpage with the specified context object. |

### [TreeSubpage: ITreeSubpage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/TreeSubpage.cs)

Base implementation of `ITreeSubpage`. Provides menu management and proper disposal of resources.

| Method                    | Return Type              | Description                                        |
|---------------------------|--------------------------|----------------------------------------------------|
| `GetChildren()`           | `IEnumerable<IRoutable>` | Returns the menu items as child routable elements. |
| `Dispose(bool disposing)` | `void`                   | Releases resources and clears the menu.            |

### [TreeSubpage&lt;TContext&gt;: ITreeSubpage&lt;TContext&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/TreeSubpage.cs)

Generic base implementation that adds context initialization support.
