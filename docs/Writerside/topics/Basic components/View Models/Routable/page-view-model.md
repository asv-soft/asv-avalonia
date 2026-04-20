# Page View Model

## Overview

[`PageViewModel<TContext>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Page/PageViewModel.cs)
is the primary abstract class for **top‑level pages** in Asv.Avalonia.
It inherits from [`ExtendableViewModel`](extendable-view-model.md) and implements the [`IPage`](#ipage) interface.

## Core Components

### Registration

Pages must be registered in the DI container via the builder. 
Use the [`Pages.Register`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/ShellMixin.cs#L95) method to register a page with its view:

```C#
// In your mixin or Program.cs:
shell.Pages.Register<HomePageViewModel, HomePageView>(HomePageViewModel.PageId);
```

### Visuals

Each page contains a Title, Icon (with color), and Status (also with color). These can be customized as needed. This
information is typically displayed in the shell tabs.

### Close Workflow

When a page is closed via a tab, the `TryCloseAsync` method is called with the force argument set to `false`. It works as follows:

1. Asks child view‑models for pending‑close reasons via `RequestChildCloseApproval`. This method raises the
   `PageCloseAttemptEvent` routing event, which view models can catch to add restrictions.
2. If reasons to prevent closing exist, it shows the `UnsavedChangesDialogPrefab`. If the user cancels the dialog, the
   method returns early.
3. Calls `RequestClose` to notify the navigation service that the page is ready to be closed.

> If force is set to true, the page ignores close restrictions.
> {style="note"}

### Command History

Each page contains its `ICommandHistory` property. It is used to store all commands executed on the page for undo and redo actions. 
See the [Command Service](command-service.md) article for more details.

## Example

A simple usage example looks like this:

```C#
public class HelloWorldPageViewModel : PageViewModel<HelloWorldPageViewModel>
{
    // A unique ID for the page, used for routing
    public const string PageId = "hello_world_page";

    // Dependencies are injected via the constructor from the IServiceCollection container
    public HelloWorldPageViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext) : base(PageId, cmd, loggerFactory, dialogService, ext)
    {
        Title = "Hello Page";

        // Use the helper method to dispose the property automatically when the VM is disposed
        SavedText = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
    }

    // A property to bind in the view
    public BindableReactiveProperty<string?> SavedText { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        // This page has no child view models (subpages or widgets),
        // so we return an empty collection.
        return [];
    }

    protected override void AfterLoadExtensions()
    {
    }
}
```

## API {collapsible="true" default-state="collapsed"}

### [IPage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Page/IPage.cs)

Represents a page in the application shell. 
Defines a common contract for navigable pages, including visual representation, command history, and controlled closing behavior.

| Property      | Type                | Description                                                             |
|---------------|---------------------|-------------------------------------------------------------------------|
| `Icon`        | `MaterialIconKind`  | Gets or sets the main icon representing the page in the tab menu.       |
| `IconColor`   | `AsvColorKind`      | Gets or sets the color of the page icon.                                |
| `Status`      | `MaterialIconKind?` | Gets or sets an optional status icon displayed alongside the page icon. |
| `StatusColor` | `AsvColorKind`      | Gets or sets the color of the status icon.                              |
| `Title`       | `string`            | Gets or sets the title of the page displayed in the tab menu.           |
| `History`     | `ICommandHistory`   | Gets the command history associated with the page.                      |
| `TryClose`    | `ICommand`          | Command that initiates an attempt to close the page.                    |

| Method                      | Return Type | Description                                                                                              |
|-----------------------------|-------------|----------------------------------------------------------------------------------------------------------|
| `TryCloseAsync(bool force)` | `ValueTask` | Attempts to close the page. When `force` is `false`, the page may request confirmation or block closing. |

### [PageViewModel&lt;TContext&gt; : IPage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Page/PageViewModel.cs)

Represents a base view model for application pages.  
Extends `ExtendableViewModel<TContext>` and provides standard page behavior such as close handling, change tracking, and integration with the shell navigation system.

| Property     | Type                             | Description                                                                               |
|--------------|----------------------------------|-------------------------------------------------------------------------------------------|
| `HasChanges` | `BindableReactiveProperty<bool>` | Indicates whether the page has unsaved changes. Used to control close confirmation logic. |

| Method                    | Return Type | Description                                  |
|---------------------------|-------------|----------------------------------------------|
| `Dispose(bool disposing)` | `void`      | Releases managed resources used by the page. |
