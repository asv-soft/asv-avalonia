# TreePage View Model

## Overview

[
`TreePageViewModel<TContext,TSubPage>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/TreePageViewModel.cs)
is the abstract class that powers *tree‑structured* pages in Asv.Avalonia.
It builds on top of [`PageViewModel`](page-view-model.md) and adds:

* A hierarchical **menu tree** (`TreeView`) that can be navigated.
* Management of **selected nodes** and the corresponding **sub‑page** (`SelectedPage`).
* Breadcrumb navigation (`BreadCrumb`).
* Commands to show/hide the side menu.
* Proper disposal of dynamically created resources.

Typical use cases include settings panels or any UI where a left‑hand tree selects a detail view on the right.

## Core Components

### Nodes

`TreePageViewModel` implements the [
`ITreePageViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/ITreePageViewModel.cs)
interface. It contains all tree nodes (displayed in the left menu):

```C#
public interface ITreePageViewModel : IDesignTimeTreePage
{
    public ObservableList<ITreePage> Nodes { get; }
}
```

Each node is described by the [
`ITreePage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Controls/TreePage/ITreePage.cs)
interface:

```C#
public interface ITreePage : IHeadlinedViewModel
{
    NavigationId ParentId { get; }
    TagViewModel? Status { get; }
    NavigationId NavigateTo { get; }
    ReactiveCommand NavigateCommand { get; }
}
```

### Navigation Flow

1. User selects a node → SelectedNode changes.
2. SelectedNodeChanged validates the navigation target, saves the current layout, and calls Maps(node.Base.NavigateTo).
3. Maps either retrieves an existing sub‑page from the MEF container (CreateSubPage) or builds a default page (
   CreateDefaultPage).
4. The new sub‑page becomes the SelectedPage; the old sub‑page (if any) is disposed.

## Usage

A typical usage example is a Settings page. It consists of a tree structure where nodes correspond to sub-pages with
concrete settings (e.g., shortcuts, units, etc.).

The following example demonstrates how to implement this pattern based on our codebase.

First, create an [
`ISettingsPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/ISettingsPage.cs)
interface for the settings page:

```C#
public interface ISettingsPage : IPage
{
    ObservableList<ITreePage> Nodes { get; }
}
```

Next, create an [
`ISettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/ISettingsSubPage.cs)
interface for the tree sub-page:

```C#
public interface ISettingsSubPage : ITreeSubpage<ISettingsPage> { }
```

Implement it in the [
`SettingsSubPage`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpage/SettingsSubPage.cs)
class:

```C#
public abstract class SettingsSubPage(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage<ISettingsPage>(id, loggerFactory),
        ISettingsSubPage
{
    public override ValueTask Init(ISettingsPage context) => ValueTask.CompletedTask;
}
```

Finally, create the [
`SettingsPageViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/SettingsPageViewModel.cs):

```C#
[ExportPage(PageId)]
public class SettingsPageViewModel
    : TreePageViewModel<ISettingsPage, ISettingsSubPage>,
        ISettingsPage
{
    // ...

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
        // ...
    }

    public override IExportInfo Source => SystemModule.Instance;
}
```

To add a sub-page to the tree view, first create an [
`ExportSettingsAttribute`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/ExportSettingsAttribute.cs):

```C#
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportSettingsAttribute : ExportAttribute
{
    public ExportSettingsAttribute(string id)
        : base(id, typeof(ISettingsSubPage)) { }
}
```

Mark your settings sub-page view models with this attribute so the MEF container can find them.

Now, create a specific sub-page, for example [
`SettingsAppearanceViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/Subpages/Appearance/SettingsAppearanceViewModel.cs):

```C#
[ExportSettings(PageId)]
public class SettingsAppearanceViewModel : SettingsSubPage
{
    // ...

    [ImportingConstructor]
    public SettingsAppearanceViewModel(
        IThemeService themeService,
        ILocalizationService localizationService,
        IDialogService dialog,
        ILoggerFactory loggerFactory
    )
        : base(PageId, loggerFactory)
    {
        // ..
    }

    // ..

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        // ..
    }

    public override IExportInfo Source => SystemModule.Instance;
}
```

However, the sub-page is not yet added to the tree. To achieve this, use the extensions functionality.

Create a [
`SettingsExtension`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/Pages/Settings/SettingsExtensions.cs)
class:

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

        // ...
    }
}
```

Here, we extend the `Nodes` property to register the new page in the tree.
