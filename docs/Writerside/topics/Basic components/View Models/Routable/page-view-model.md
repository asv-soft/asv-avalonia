# Page View Model

## Overview

[`PageViewModel<TContext>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Page/PageViewModel.cs)
is the primary abstract class for **top‑level pages** in Asv.Avalonia.
It inherits from [`ViewModel<TContext>`](extendable-view-model.md) and implements the [`IPage`](#ipage) interface.

## Core Components

### Registration

Pages must be registered in the DI container via the builder. 
Use the [`Pages.Register`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/PagesRegistrations.cs#L38) method to register a page with its view:

```C#
// In your mixin or Program.cs:
shell.Pages.Register<HomePageViewModel, HomePageView>(HomePageViewModel.PageId);
```

### Visuals

Each page contains a Header, Icon (with color), and Status (also with color). These can be customized as needed. This
information is typically displayed in the shell tabs.

### Close Workflow

When a page is closed via a tab, the `TryCloseAsync` method is called with the force argument set to `false`. It works
as follows:

1. Asks child view‑models for pending‑close reasons via `RequestChildCloseApproval`. This method raises the
   `PageCloseAttemptEvent` routing event, which view models can catch to add restrictions.
2. If reasons to prevent closing exist, it shows the `UnsavedChangesDialogPrefab`. If the user cancels the dialog, the
   method returns early.
3. Calls `RequestClose`, which raises the bubbling `PageCloseRequestedEvent` to notify the shell that the page is ready
   to be closed. The shell handles the event and performs the actual close.

> If force is set to true, the page ignores close restrictions.
> {style="note"}

### Undo History

Each page exposes an `UndoHistory` property of type `IUndoHistory<IViewModel>`, backed by the `IUndoHistoryStore`
supplied through `IPageContext`. It records the operations performed on the page for undo and redo actions.

## Example

A simple usage example looks like this:

```C#
public class HelloWorldPageViewModel : PageViewModel<HelloWorldPageViewModel>
{
    // A page type ID used for routing; uniqueness is validated at runtime
    public const string PageId = "hello_world_page";

    // Dependencies are injected via the constructor from the IServiceCollection container
    public HelloWorldPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext) : base(PageId, context, loggerFactory, dialogService, ext)
    {
        Header = "Hello Page";

        // Use the helper method to dispose the property automatically when the VM is disposed
        SavedText = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
    }

    // A property to bind in the view
    public BindableReactiveProperty<string?> SavedText { get; }

    protected override void AfterLoadExtensions()
    {
    }
}
```

This page has no child view models (subpages or widgets), so it does not override `GetChildren()` — the base
implementation already returns an empty collection.

## API {collapsible="true" default-state="collapsed"}

### [IPage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Page/IPage.cs)

Represents a page in the application shell. Defines a common contract for navigable pages, including visual
representation (icon, status, header), undo history, layout management, and controlled closing behavior.

| Property        | Type                         | Description                                                            |
|-----------------|------------------------------|------------------------------------------------------------------------|
| `Icon`          | `MaterialIconKind?`          | Gets or sets the main icon representing the page in the tab menu.      |
| `IconColor`     | `AsvColorKind`               | Gets or sets the color of the page icon.                               |
| `Status`        | `MaterialIconKind?`          | Gets an optional status icon displayed alongside the page icon.        |
| `StatusColor`   | `AsvColorKind`               | Gets the color of the status icon.                                     |
| `Header`        | `string?`                    | Gets or sets the header (title) of the page displayed in the tab menu. |
| `UndoHistory`   | `IUndoHistory<IViewModel>`   | Gets the undo history associated with the page.                        |
| `LayoutManager` | `ILayoutManager<IViewModel>` | Gets the layout manager associated with the page.                      |
| `TryClose`      | `ICommand`                   | Command that initiates an attempt to close the page.                   |

`Status` and `StatusColor` are get-only on the contract; `PageViewModel<TContext>` exposes them as settable.

| Method                      | Return Type | Description                                                                                              |
|-----------------------------|-------------|----------------------------------------------------------------------------------------------------------|
| `TryCloseAsync(bool force)` | `ValueTask` | Attempts to close the page. When `force` is `false`, the page may request confirmation or block closing. |

### [IPageContext](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Page/IPageContext.cs)

Bundles the per-page construction data: the navigation arguments plus the undo and layout stores. `PageContext` is the
standard implementation; `NullPageContext.Instance` is used at design time and returns `NavArgs.Empty` together with the
null undo and layout stores.

| Property      | Type                | Description                                                                 |
|---------------|---------------------|-----------------------------------------------------------------------------|
| `NavArgs`     | `NavArgs`           | Gets the navigation arguments, forwarded to the base `ViewModel<TContext>`. |
| `UndoStore`   | `IUndoHistoryStore` | Gets the store backing the page's `UndoHistory`.                            |
| `LayoutStore` | `ILayoutStore`      | Gets the store backing the page's `LayoutManager`.                          |

### [PageViewModel&lt;TContext&gt; : IPage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Page/PageViewModel.cs)

Represents a base view model for application pages.  
Extends `ViewModel<TContext>` and provides standard page behavior such as close handling with restriction approval,
undo history, layout management, and integration with the shell via routed events.
Constrained to `where TContext : class, IPage`.

| Constructor                                                                                                                             | Description                                                                            |
|-----------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------|
| `PageViewModel(string typeId, IPageContext context, ILoggerFactory loggerFactory, IDialogService dialogService, IExtensionService ext)` | Protected. The `context` supplies the navigation arguments and the undo/layout stores. |
