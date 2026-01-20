# Tree Page View Model

## Overview

[`TreePageViewModel<TContext,TSubPage>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreePageViewModel.cs) is an abstract class that powers *tree‑structured* pages in Asv.Avalonia. 
It builds on top of [`PageViewModel`](page-view-model.md) and adds:

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
2. `SelectedNodeChanged` validates the navigation target, saves the current layout, and calls `Navigate(node.Base.NavigateTo)`.
3. `Navigate` either retrieves an existing subpage from the MEF container (`CreateSubPage`) or builds a default page (`CreateDefaultPage`).
4. The new subpage becomes the `SelectedPage`; the old subpage (if any) is disposed.

### Tree Structure

The tree is built from `Nodes` collection which contains `ITreePage` items. Each node has a navigation ID and can have children, forming a hierarchical structure.

### Breadcrumb Navigation

The breadcrumb automatically updates based on the selected node, showing the path from the root to the current selection.

### Menu Visibility

You can show or hide the side menu using `ShowMenuCommand` and `HideMenuCommand`. The `IsMenuVisible` property reflects the current state.

### Layout Management

The tree page automatically saves and restores its state, including the selected node. When a node is selected:

1. The current layout is saved to memory.
2. The layout is saved to file (if supported).
3. Navigation to the new subpage occurs.
4. The new subpage's layout is loaded.

When the page is reopened, it automatically restores the previously selected node using the saved configuration.

### Configuration

TreePageViewModel uses `TreePageViewModelConfig` to persist state between sessions. This configuration stores:

* `SelectedNodeId` - The navigation ID of the currently selected node.

The configuration is automatically managed through the layout service's save/load events.

### Default Page

If no specific subpage is found for a navigation ID, the `CreateDefaultPage()` method is called. By default, it creates a `GroupTreePageItemViewModel` which displays all child nodes of the selected tree node as clickable cards.

## Example

A typical usage example is a Settings page. It consists of a tree structure where nodes correspond to subpages with concrete settings (e.g., shortcuts, units, etc.).

First, create an [`ISettingsPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/ISettingsPage.cs) interface for the settings page:

```C#
public interface ISettingsPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
}
```

Next, create the [`SettingsPageViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/SettingsPageViewModel.cs):

```C#
[ExportPage(PageId)]
public class SettingsPageViewModel
    : TreePageViewModel<ISettingsPage, ISettingsSubPage>,
        ISettingsPage
{
    public const string PageId = "settings";

    [ImportingConstructor]
    public SettingsPageViewModel(
        ICommandService svc,
        IContainerHost host,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, svc, host, layoutService, loggerFactory, dialogService)
    {
        Title = "Settings";
        Icon = MaterialIconKind.Cog;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
```

To add nodes to the tree view, use the extensions functionality. Create a [`SettingsExtension`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/SettingsExtensions.cs) class:

```C#
[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsExtension(ILoggerFactory loggerFactory) : IExtensionFor<ISettingsPage>
{
    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                SettingsAppearanceViewModel.PageId,
                RS.SettingsAppearanceViewModel_Name,
                MaterialIconKind.ThemeLightDark,
                SettingsAppearanceViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );

        // Add more nodes as needed
    }
}
```

Here, we extend the `Nodes` property to register new pages in the tree.

For information on how to create subpages that are displayed when tree nodes are selected, see [`TreeSubpage`](tree-subpage.md).

## API {collapsible="true" default-state="collapsed"}

### [ITreePageViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/ITreePageViewModel.cs)

| Property          | Type                                                                     | Description                                                     |
|-------------------|--------------------------------------------------------------------------|-----------------------------------------------------------------|
| `Nodes`           | `ObservableList<ITreePage>`                                              | Gets the collection of root nodes in the tree.                  |

### [TreePageViewModel&lt;TContext, TSubPage&gt;: ITreePageViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreePageViewModel.cs)

Represents a page with a tree-based navigation structure. Extends `PageViewModel<TContext>` and manages a tree menu with corresponding subpages.

| Property          | Type                                                                     | Description                                                     |
|-------------------|--------------------------------------------------------------------------|-----------------------------------------------------------------|
| `ShowMenuCommand` | `ReactiveCommand`                                                        | Command to show the side menu.                                  |
| `HideMenuCommand` | `ReactiveCommand`                                                        | Command to hide the side menu.                                  |
| `IsMenuVisible`   | `bool`                                                                   | Indicates whether the side menu is currently visible.           |
| `TreeView`        | `ObservableTree<ITreePage, NavigationId>`                                | Gets the tree structure for navigation.                         |
| `SelectedPage`    | `BindableReactiveProperty<ITreeSubpage?>`                                | Gets or sets the currently displayed subpage.                   |
| `BreadCrumb`      | `ISynchronizedViewList<BreadCrumbItem>`                                  | Gets the breadcrumb navigation items.                           |
| `SelectedNode`    | `BindableReactiveProperty<ObservableTreeNode<ITreePage, NavigationId>?>` | Gets or sets the currently selected tree node.                  |

| Method                           | Return Type                | Description                                                          |
|----------------------------------|----------------------------|----------------------------------------------------------------------|
| `Navigate(NavigationId id)`      | `ValueTask<IRoutable>`     | Navigates to the subpage with the specified navigation ID.           |
| `GetChildren()`                  | `IEnumerable<IRoutable>`   | Returns the root nodes as child routable elements.                   |
| `CreateDefaultPage()`            | `ITreeSubpage?`            | Creates a default subpage when no specific subpage is found.         |
| `CreateSubPage(NavigationId id)` | `ValueTask<ITreeSubpage?>` | Creates or retrieves a subpage from the MEF container by ID.         |
| `GetContext()`                   | `TContext`                 | Returns the context object for the page (typically the page itself). |
| `AfterLoadExtensions()`          | `void`                     | Called after all extensions have been loaded.                        |
| `Dispose(bool disposing)`        | `void`                     | Releases managed resources including the current subpage.            |