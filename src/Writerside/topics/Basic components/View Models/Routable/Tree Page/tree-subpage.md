# Tree Subpage

## Overview

[`TreeSubpage`](#treesubpage-itreesubpage) is an abstract base class for content pages displayed within a
[`TreePageViewModel`](tree-page-view-model.md).

It extends [`ViewModel`](view-model.md) and implements the [`ITreeSubpage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/ITreeSubpage.cs) interface.

TreeSubpage serves as the detail view that appears on the right side when a user selects a node in the tree menu. It
includes its own menu system.

## Core Components

### Menu System

Each subpage has its own `Menu` collection and `MenuView` tree structure. This allows subpages to have their own toolbar
or contextual menu items.

The menu items are automatically disposed when the subpage is disposed, and they inherit the routing parent from the
subpage itself.

### Generic Context

`TreeSubpage<TContext>` is a generic variant that receives an `ITreeSubPageContext<TContext>` through its constructor.
The context carries both the navigation `Args` — forwarded to the base `TreeSubpage` — and the `Context` object itself,
which is typically the parent page that hosts the subpage. The base class does not store or expose `Context`; a derived
class that needs it after construction must retain it explicitly.

## Example

A typical usage pattern involves creating a subpage interface, a base implementation, and then concrete subpages.

First, create an interface for your tree subpage (e.g., [`ISettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/ISettingsSubPage.cs)). The context type is bound on the base *class*, not on the interface:

```C#
public interface ISettingsSubPage : ITreeSubpage { }
```

Next, implement a base class (e.g., [`SettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/SettingsSubPage.cs)):

```C#
public abstract class SettingsSubPage(string typeId, ITreeSubPageContext<ISettingsPage> context)
    : TreeSubpage<ISettingsPage>(typeId, context),
        ISettingsSubPage { }
```

Now create a concrete subpage view model. Simply inherit from the base class created above and pass the context through:

```C#
public class SettingsUnitsViewModel : SettingsSubPage
{
    public const string PageId = "units";

    public SettingsUnitsViewModel(
        ITreeSubPageContext<ISettingsPage> context,
        ILoggerFactory loggerFactory
    )
        : base(PageId, context)
    {
        // Initialize your properties and commands here
    }
}
```

>  If the subpage needs extensibility (e.g., allowing plugins to add sections), inherit from
> [`ExtendableTreeSubpage`](extendable-tree-subpage.md) instead.
> {style="note"}

Each subpage needs a tree menu node — this is what appears in the left-hand tree. Create a class that inherits from
`TreePageMenuItem`:

```C#
public class SettingsUnitTreePageMenu : TreePageMenuItem
{
    public SettingsUnitTreePageMenu()
        : base(
            SettingsUnitsViewModel.PageId,          // node ID
            "Units",                                // display name
            MaterialIconKind.KeyboardSettings,      // icon
            new NavId(SettingsUnitsViewModel.PageId), // navigation target (subpage ID)
            NavId.Empty                             // parent node (empty = root level)
        ) { }
}
```

Finally, register the subpage, its view, and the tree menu node. The settings builder wraps all three registrations into
one call:

```C#
public static class UnitsSubPageRegistrations
{
    extension(SettingsPageRegistrations.Builder builder)
    {
        public SettingsPageRegistrations.Builder RegisterUnitsSubPage()
        {
            return builder.AddSubPage<
                SettingsUnitsViewModel,
                SettingsUnitsView,
                SettingsUnitTreePageMenu
            >(SettingsUnitsViewModel.PageId);
        }
    }
}
```

> A subpage cannot be registered with a plain `AddKeyedTransient`: its constructor takes an
> `ITreeSubPageContext<TContext>`, which is a per-navigation runtime argument rather than a registered service.
> Registration goes through the `TreePage` builder, which is what `AddSubPage` calls for you.
> {style="note"}

## API {collapsible="true" default-state="collapsed"}

### [ITreeSubpage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/ITreeSubpage.cs)

Represents a subpage that can be displayed in a tree-based page structure. 
Extends `IViewModel` to provide identity, parent/child routing, undo and layout support.

| Property   | Type                        | Description                                                 |
|------------|-----------------------------|-------------------------------------------------------------|
| `MenuView` | `MenuTree`                  | Gets the tree structure for the subpage's menu.             |
| `Menu`     | `ObservableList<IMenuItem>` | Gets the collection of menu items associated with the page. |

### [ITreeSubPageContext&lt;TContext&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/ITreeSubpage.cs)

Carries what a subpage receives at construction. `TreeSubPageContext<TContext>` is the standard implementation;
`NullTreeSubPageContext<TContext>` is used at design time.

| Property  | Type       | Description                                                       |
|-----------|------------|-------------------------------------------------------------------|
| `Args`    | `NavArgs`  | Navigation arguments, forwarded to the base `TreeSubpage`.        |
| `Context` | `TContext` | The page that hosts the subpage.                                  |

### [TreeSubpage: ITreeSubpage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/TreeSubpage.cs)

Base implementation of `ITreeSubpage`. Provides menu management and proper disposal of resources.

| Constructor                                | Description                                       |
|--------------------------------------------|---------------------------------------------------|
| `TreeSubpage(string typeId, NavArgs args)` | Protected. Creates the subpage and its menu tree. |

| Method                    | Return Type               | Description                                        |
|---------------------------|---------------------------|----------------------------------------------------|
| `GetChildren()`           | `IEnumerable<IViewModel>` | Returns the menu items as child view models.       |
| `Dispose(bool disposing)` | `void`                    | Releases resources and clears the menu.            |

### [TreeSubpage&lt;TContext&gt;: TreeSubpage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreeSubpage/TreeSubpage.cs)

Generic base implementation that binds a page context type. It adds no members of its own — it forwards `context.Args`
to the base constructor. Constrained to `where TContext : class, IPage`.

| Constructor                                                                   | Description                                     |
|-------------------------------------------------------------------------------|-------------------------------------------------|
| `TreeSubpage<TContext>(string typeId, ITreeSubPageContext<TContext> context)` | Protected. Forwards `context.Args` to the base. |
