# Tree Subpage

## Overview

[`TreeSubpage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreeSubpage/TreeSubpage.cs) is an abstract base class for content pages displayed within a [`TreePageViewModel`](tree-page-view-model.md).

It extends [`RoutableViewModel`](routable-view-model.md) and implements the [`ITreeSubpage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreeSubpage/ITreeSubpage.cs) interface.

TreeSubpage serves as the detail view that appears on the right side when a user selects a node in the tree menu. It includes its own menu system and can be extended through the MEF container.

## Core Components

### Menu System

Each subpage has its own `Menu` collection and `MenuView` tree structure. This allows subpages to have their own toolbar or contextual menu items.

The menu items are automatically disposed when the subpage is disposed, and they inherit the routing parent from the subpage itself.

### Generic Context

`TreeSubpage<TContext>` is a generic variant that receives a context object during initialization via the `Init` method. This context is typically the parent page that hosts the subpage, allowing the subpage to access shared data or services.

### Export Information

Each subpage must provide `Source` property that returns `IExportInfo`, which identifies the module where the subpage is defined. This is used by the MEF container for dependency resolution.

## Example

A typical usage pattern involves creating a subpage interface, a base implementation, and then concrete subpages.

First, create an interface for your tree subpage (e.g., [`ISettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/ISettingsSubPage.cs)):

```C#
public interface ISettingsSubPage : ITreeSubpage<ISettingsPage> 
{ 
}
```

Next, implement a base class (e.g., [`SettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpage/SettingsSubPage.cs)):

```C#
public abstract class SettingsSubPage(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage<ISettingsPage>(id, loggerFactory),
        ISettingsSubPage
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;
}
```

Now create a concrete subpage view model (e.g., [`SettingsAppearanceViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/Appearance/SettingsAppearanceViewModel.cs)):

```C#
[ExportSettings(PageId)]
public class SettingsAppearanceViewModel : SettingsSubPage
{
    public const string PageId = "appearance";

    [ImportingConstructor]
    public SettingsAppearanceViewModel(
        IThemeService themeService,
        ILocalizationService localizationService,
        IDialogService dialog,
        ILoggerFactory loggerFactory
    )
        : base(PageId, loggerFactory)
    {
        // Initialize your properties and commands here
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        // Return child routable items if any
        return [];
    }

    public override IExportInfo Source => SystemModule.Instance;
}
```

To make the subpage discoverable by MEF, create a custom export attribute (e.g., [`ExportSettingsAttribute`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/ExportSettingsAttribute.cs)):

```C#
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportSettingsAttribute : ExportAttribute
{
    public ExportSettingsAttribute(string id)
        : base(id, typeof(ISettingsSubPage)) { }
}
```

## API {collapsible="true" default-state="collapsed"}

### [ITreeSubpage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreeSubpage/ITreeSubpage.cs)

Represents a subpage that can be displayed in a tree-based page structure. 
Extends `IRoutable` and `IExportable` to provide routing and MEF export capabilities.

| Property   | Type                        | Description                                                 |
|------------|-----------------------------|-------------------------------------------------------------|
| `MenuView` | `MenuTree`                  | Gets the tree structure for the subpage's menu.             |
| `Menu`     | `ObservableList<IMenuItem>` | Gets the collection of menu items associated with the page. |

### [ITreeSubpage&lt;TContext&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreeSubpage/ITreeSubpage.cs)

Generic variant of `ITreeSubpage` that receives a context during initialization.

| Method                   | Return Type | Description                                                |
|--------------------------|-------------|------------------------------------------------------------|
| `Init(TContext context)` | `ValueTask` | Initializes the subpage with the specified context object. |

### [TreeSubpage: ITreeSubpage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreeSubpage/TreeSubpage.cs)

Base implementation of `ITreeSubpage`. Provides menu management and proper disposal of resources.

| Property   | Type                        | Description                                     |
|------------|-----------------------------|-------------------------------------------------|
| `Source`   | `IExportInfo`               | Gets export metadata (abstract, must override). |

| Method                    | Return Type              | Description                                        |
|---------------------------|--------------------------|----------------------------------------------------|
| `GetChildren()`           | `IEnumerable<IRoutable>` | Returns the menu items as child routable elements. |
| `Dispose(bool disposing)` | `void`                   | Releases resources and clears the menu.            |

### [TreeSubpage&lt;TContext&gt;: ITreeSubpage&lt;TContext&gt;](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreeSubpage/TreeSubpage.cs)

Generic base implementation that adds context initialization support.
