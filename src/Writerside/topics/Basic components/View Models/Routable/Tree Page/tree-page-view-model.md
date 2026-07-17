# Tree Page View Model

## Overview

[`TreePageViewModel<TContext,TSubPage>`](#treepageviewmodel-tcontext-tsubpage-itreepageviewmodel) is an abstract class
that powers *tree‑structured* pages in Asv.Avalonia. It builds on top of [`PageViewModel`](page-view-model.md) and adds:

* A hierarchical **menu tree** (`TreeView`) that can be navigated.
* Management of **selected nodes** and the corresponding **subpage** (`SelectedPage`).
* Breadcrumb navigation (`BreadCrumb`).
* Commands to show/hide the side menu.
* Proper disposal of dynamically created resources.

Typical use cases include settings panels or any UI where a left‑hand tree selects a detail view on the right.

The subpages displayed on the right are implemented using [`TreeSubpage`](tree-subpage.md).

## Core Components

### Navigation Flow

1. User selects a node → `SelectedNode` changes.
2.  `SelectedNodeChanged` skips null and internal navigations, rebuilds the breadcrumb from the node's path to the root,
   and calls `Navigate(node.Base.NavigateTo)`.
3.  `Navigate` either creates a new subpage through the DI container (`CreateSubPage`) or builds a default page
   (`CreateDefaultPage`).
4. The new subpage becomes the `SelectedPage`; the old subpage (if any) is disposed.

### Tree Structure

`Nodes` is a *flat* collection of `ITreePageMenuItem`: root items and nested items alike are added straight into it. The
hierarchy is not expressed by nesting — `TreeView` derives it from each item's `Id` and `ParentId`, treating
`NavId.Empty` as the root.

### Breadcrumb Navigation

The breadcrumb automatically updates based on the selected node, showing the path from the root to the current
selection.

### Menu Visibility

You can show or hide the side menu using `ShowMenuCommand` and `HideMenuCommand`. The `IsMenuVisible` property reflects
the current state.

### Layout Management

The tree page automatically saves and restores its state through the `Layout` controller. In `AfterLoadExtensions` it
registers two layout values:

* the selected node, stored as the node key string under `nameof(SelectedNode)`;
* `IsMenuVisible`, stored under `nameof(IsMenuVisible)`.

Both are saved whenever they change, and loaded once the page is attached to the shell — so when the page is reopened,
it restores the previously selected node and the menu state.

### Default Page

If no specific subpage is found for a navigation ID, the `CreateDefaultPage()` method is called. By default, it creates
a `GroupTreePageItemViewModel` which displays all child nodes of the selected tree node as clickable cards.

## Example

A typical usage example is a Settings page. It consists of a tree structure where nodes correspond to subpages with
concrete settings (e.g., shortcuts, units, etc.).

First, create an [`ISettingsPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/ISettingsPage.cs) interface for the settings page. `ITreePageViewModel` already supplies the 
`Nodes` collection, so the interface only has to name the page:

```C#
public interface ISettingsPage : ITreePageViewModel;
```

Next, create the [`SettingsPageViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/SettingsPageViewModel.cs):

```C#
public class SettingsPageViewModel
    : TreePageViewModel<ISettingsPage, ISettingsSubPage>,
        ISettingsPage
{
    public const string PageId = "settings";

    public SettingsPageViewModel(
        IPageContext context,
        IServiceProvider host,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, host, loggerFactory, dialogService, ext)
    {
        Header = "Settings";
        Icon = MaterialIconKind.Settings;
    }
}
```

To add nodes to the tree view, use extensions. The [`DefaultSettingsExtension`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/DefaultSettingsExtension.cs) resolves all `ITreePageMenuItem` 
services keyed by the parent page ID and adds them to the tree:

```C#
public class DefaultSettingsExtension(
    [FromKeyedServices(SettingsPageViewModel.PageId)] IEnumerable<ITreePageMenuItem> items
) : IExtensionFor<ISettingsPage>
{
    public const string StaticId = "ext.settings.tree";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        foreach (var treePageMenuItem in items)
        {
            context.Nodes.Add(treePageMenuItem);
            treePageMenuItem.DisposeItWith(contextDispose);
        }
    }
}
```

Tree menu nodes are registered as keyed services in the DI container (keyed by the parent page ID), so the extension
automatically picks them up.

### Registration

All components must be registered in the builder chain. A typical registration for a tree page:

```C#
// Register the tree page itself
builder.Shell.Pages.Register<SettingsPageViewModel, TreePageView>(SettingsPageViewModel.PageId);

// Register the extension that adds a button to the home page
builder.Extensions.Register<IHomePage, HomePageSettingsExtension>();
```

A subpage needs its view model, its view, and its tree menu node. The first two are registered together by the
`TreePage` builder, which wires up the keyed view model (with its tree subpage context) and the view locator entry:

```C#
// Register the subpage view model and its view
builder.TreePage.Register<
    ISettingsPage,
    ISettingsSubPage,
    SettingsAppearanceViewModel,
    SettingsAppearanceView
>(SettingsAppearanceViewModel.PageId);

// Register the tree menu node (keyed by the parent page ID)
builder.Services.AddKeyedTransient<ITreePageMenuItem, AppearanceSettingTreePageMenu>(
    SettingsPageViewModel.PageId);
```

In practice, the registration builder [`SettingsPageRegistrations.Builder`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/SettingsPageRegistrations.cs) wraps both calls into a single one:

```C#
builder.AddSubPage<
    SettingsAppearanceViewModel,
    SettingsAppearanceView,
    AppearanceSettingTreePageMenu
>(SettingsAppearanceViewModel.PageId);
```

For information on how to create subpages that are displayed when tree nodes are selected, see
[`TreeSubpage`](tree-subpage.md).

## API {collapsible="true" default-state="collapsed"}

### [ITreePageViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/ITreePageViewModel.cs)

| Property | Type                                | Description                                                                                                                         |
|----------|-------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| `Nodes`  | `ObservableList<ITreePageMenuItem>` | Gets the flat collection of all menu items. The hierarchy is built from each item's `Id`/`ParentId`, not from nesting in this list. |

### [TreePageViewModel&lt;TContext, TSubPage&gt;: ITreePageViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/TreePage/TreePageViewModel.cs)

Represents a page with a tree-based navigation structure. Extends `PageViewModel<TContext>` and manages a tree menu with
corresponding subpages.

#### `TreePageViewModel` constructor

| Constructor                                                                                                                                                             | Description                                                                                                                               |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `TreePageViewModel(string typeId, IPageContext context, IServiceProvider container, ILoggerFactory loggerFactory, IDialogService dialogService, IExtensionService ext)` | Protected. Creates the page along with its `TreeView` menu, breadcrumb source and menu commands. `container` is used to resolve subpages. |

| Property          | Type                                                                      | Description                                                                                                                 |
|-------------------|---------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------|
| `TreeHeaderIcon`  | `MaterialIconKind?`                                                       | Gets or sets the icon shown in the tree menu header. The default view falls back to the page's `Icon` when it is not set.   |
| `TreeHeader`      | `string?`                                                                 | Gets or sets the text shown in the tree menu header. The default view falls back to the page's `Header` when it is not set. |
| `ShowMenuCommand` | `ReactiveCommand`                                                         | Command to show the side menu.                                                                                              |
| `HideMenuCommand` | `ReactiveCommand`                                                         | Command to hide the side menu.                                                                                              |
| `IsMenuVisible`   | `bool`                                                                    | Indicates whether the side menu is currently visible.                                                                       |
| `TreeView`        | `ObservableTree<ITreePageMenuItem, NavId>`                                | Gets the tree structure for navigation.                                                                                     |
| `SelectedPage`    | `BindableReactiveProperty<ITreeSubpage?>`                                 | Gets or sets the currently displayed subpage.                                                                               |
| `BreadCrumb`      | `ISynchronizedViewList<BreadCrumbItem>`                                   | Gets the breadcrumb navigation items.                                                                                       |
| `SelectedNode`    | `BindableReactiveProperty<ObservableTreeNode<ITreePageMenuItem, NavId>?>` | Gets or sets the currently selected tree node.                                                                              |
| `Nodes`           | `ObservableList<ITreePageMenuItem>`                                       | Gets the flat collection from which `TreeView` is built.                                                                    |
| `Context`         | `TContext`                                                                | Protected. The context object extensions are applied to (typically the page itself).                                        |

| Method                      | Return Type               | Description                                                                          |
|-----------------------------|---------------------------|--------------------------------------------------------------------------------------|
| `Navigate(NavId id)`        | `ValueTask<IViewModel>`   | Navigates to the subpage with the specified navigation ID.                           |
| `GetChildren()`             | `IEnumerable<IViewModel>` | Returns every item from `Nodes` plus the currently selected subpage (if any).        |
| `CreateDefaultPage()`       | `ITreeSubpage?`           | Creates a default subpage when no specific subpage is found.                         |
| `CreateSubPage(NavId id)`   | `ITreeSubpage?`           | Creates a new subpage instance through the DI container by ID.                       |
| `AfterLoadExtensions()`     | `void`                    | Called after all extensions have been loaded. Registers the page's layout values.    |
