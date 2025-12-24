# Page View Model

## Overview

[
`PageViewModel<TContext>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/PageViewModel.cs)
is the primary abstract class for **top‑level pages** in Asv.Avalonia.
It inherits from [`ExtendableViewModel`](extendable-view-model.md) (which itself inherits from [
`RoutableViewModel`](routable-view-model.md)) and implements the `IPage` interface. It provides every page with:

* Routing primitives (like `NavigationId`).
* Visual representation (`Icon`, `IconColor`, `Status`, `StatusColor`, `Title`).
* Command history (`History`) for undo/redo operations.
* A built‑in *Close* workflow with unsaved‑changes handling (`TryClose`, `TryCloseAsync`).
* MEF2 extensibility through `ExtendableViewModel`.
* Export metadata via the abstract `Source` property.

Most of this functionality comes from base classes like [`RoutableViewModel`](routable-view-model.md), [
`DisposableViewModel`](disposable-view-model.md), and [`ViewModelBase`](view-model-base.md), so refer to their
documentation for more details.

## Core Components

### IPage Interface

`PageViewModel` implements the [
`IPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/IPage.cs) interface with the
following properties:

```C#
public interface IPage : IRoutable, IExportable
{
    MaterialIconKind Icon { get; }
    AsvColorKind IconColor { get; }

    MaterialIconKind? Status { get; }
    AsvColorKind StatusColor { get; }
    string Title { get; }
    ICommandHistory History { get; }
    ICommand TryClose { get; }
    ValueTask TryCloseAsync(bool force);
}
```

### ExportPage attribute

You must apply the [
`ExportPage(pageId)`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/ExportPageAttribute.cs)
attribute to your page classes. This helps the MEF container find and register the view models.

### Visuals

Each page contains a Title, Icon (with color), and Status (also with color). These can be customized as needed. This
information is typically displayed in the shell tabs.

### Close Workflow

When a page is closed via a tab, the `TryCloseAsync` method (with force = false) is called. It performs the following
steps:

1. Asks child view‑models for pending‑close reasons via `RequestChildCloseApproval`. This method raises the
   `PageCloseAttemptEvent` routing event, which view models can catch to add restrictions.
2. If reasons to prevent closing exist, it shows the `UnsavedChangesDialogPrefab`. If the user cancels the dialog, the
   method returns early.
3. Calls `RequestClose` to notify the navigation service that the page is ready to be closed.

If force is set to true, the page ignores close restrictions.

### Usage

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
    
    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        // This page has no child view models (sub-pages or widgets), 
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
