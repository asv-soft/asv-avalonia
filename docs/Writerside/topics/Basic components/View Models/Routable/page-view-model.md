# Page View Model

## Overview

[`PageViewModel<TContext>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/PageViewModel.cs)
is the primary abstract class for **top‑level pages** in Asv.Avalonia.
It inherits from [`ExtendableViewModel`](extendable-view-model.md) and implements the [`IPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/IPage.cs) interface.

## Core Components

### ExportPage attribute

You must apply the [`ExportPage(pageId)`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/ExportPageAttribute.cs) attribute to your page classes. This helps the MEF container find and register the view models.

```C#
[ExportPage(PageId)]
public class HomePageViewModel : PageViewModel<HomePageViewModel>
{
    public const string PageId = "home";
    
    // ...
}
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
[ExportPage(PageId)]
public class HelloWorldPageViewModel : PageViewModel<HelloWorldPageViewModel>
{
    // A unique ID for the page, used for routing
    public const string PageId = "hello_world_page";
    
    // You can request dependencies from the MEF container via the constructor
    [ImportingConstructor]
    public HelloWorldPageViewModel(
        ICommandService cmd, 
        ILoggerFactory loggerFactory, 
        IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
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

    // Metadata about the module where this page is defined
    public override IExportInfo Source => SystemModule.Instance;
}
```

## API {collapsible="true" default-state="collapsed"}

### [IPage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/IPage.cs)

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

### [PageViewModel&lt;TContext&gt; : IPage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/PageViewModel.cs)

Represents a base view model for application pages.  
Extends `ExtendableViewModel<TContext>` and provides standard page behavior such as close handling, change tracking, and integration with the shell navigation system.

| Property     | Type                             | Description                                                                               |
|--------------|----------------------------------|-------------------------------------------------------------------------------------------|
| `HasChanges` | `BindableReactiveProperty<bool>` | Indicates whether the page has unsaved changes. Used to control close confirmation logic. |
| `Source`     | `IExportInfo`                    | Gets export metadata describing the origin module of the page.                            |

| Method                    | Return Type | Description                                  |
|---------------------------|-------------|----------------------------------------------|
| `Dispose(bool disposing)` | `void`      | Releases managed resources used by the page. |
