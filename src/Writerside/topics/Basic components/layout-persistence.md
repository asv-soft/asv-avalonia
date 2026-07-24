# Layout Persistence

## Overview

Layout persistence remembers presentation state such as selected items, panel sizes, window geometry,
filters, and scroll positions. It lets an application restore the interface when a page or the whole
application is opened again.

Layout persistence is intended for UI state. Use application configuration or a domain-specific store
for business settings and user data that should not depend on a view model's navigation path.

The underlying layout implementation belongs to the
[`Asv.Modeling` project](https://github.com/asv-soft/asv-common/tree/main/src/Asv.Modeling)
in the `asv-common` repository. Asv.Avalonia integrates it with its view-model tree, standard shell
and pages, application data directories, and Avalonia controls. This guide focuses on that integration.

Asv.Avalonia provides the following integration points:

| Integration point              | Behavior                                                                     |
|--------------------------------|------------------------------------------------------------------------------|
| `IViewModel.Layout`            | Every framework view model exposes a lazily created `ILayoutController`.     |
| `ShellViewModel.LayoutManager` | Handles shell-level layout values and owns the shell JSON store.             |
| `PageViewModel.LayoutManager`  | Gives each page and its descendants a separate layout store.                 |
| `IAppPath` integration         | Places standard stores under the application's `data/layout` directory.      |
| `ViewLayoutMixin`              | Connects Avalonia controls to the `Layout` controller of their data context. |
| Control helpers                | Persist common `Grid` and `Workspace` dimensions without custom callbacks.   |

Calling `ILayoutSink.LoadAsync` or `ILayoutSink.SaveAsync` raises a routed event from the owning view
model. The nearest page or shell manager resolves the owner's relative navigation path and delegates
the operation to its store.

Applications built on the standard shell and page classes do not register a manager or store
themselves.

## How It Works

### Lifecycle

Registration, loading, saving, and writing to disk are separate operations:

| Stage        | When it happens                                                                                                                                                                                                                     |
|--------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Registration | Application code declares a `layoutId`, data type, and load callback. Registration alone does not load or save anything.                                                                                                            |
| Loading      | View models usually call `LoadWhenRootAttached`, which loads after they join the shell tree. Control helpers load when their `DataContext` becomes an `IViewModel`. A caller may also use `LoadAsync` or `LoadAllAsync` explicitly. |
| Saving       | `ILayoutSink.SaveAsync` routes the snapshot to the nearest layout manager. With the default `JsonTokenLayoutStore`, the manager updates the store's in-memory snapshot.                                                             |
| Disk flush   | The default `JsonTokenLayoutStore` writes dirty data every five seconds, when `Flush` is called, or when the store is disposed.                                                                                                     |

There is no single application-wide "save layout" moment. Trigger-based helpers save each registered
value independently when its trigger fires. A caller using `ILayoutSink<TData>` directly decides when
to call `SaveAsync`. For example, `ShellWindow` uses a one-second throttled trigger for position, size,
and window-state changes; the grid helper reacts to column width and splitter changes; and the workspace
helper reacts to `WorkspaceChanged`. A custom view model decides which user actions emit its trigger.

For a `Register(..., save, trigger)` registration, saving follows this sequence:

1. `trigger` emits a value.
2. The helper calls `save()` to capture the current UI state as a DTO.
3. If the callback returns a DTO, the helper passes it to `ILayoutSink.SaveAsync`.
4. `JsonTokenLayoutStore` updates its in-memory snapshot for that navigation path and layout ID.

The `layout.json` file is updated separately. The default store flushes pending snapshots on its
five-second timer, after an explicit `Flush()` call, or during disposal. Disposing a page flushes its
store; disposing the shell flushes the shell store.

Loading a snapshot may update the same properties that produce save notifications. Suppress those
notifications while restoring, or make the trigger represent user-originated changes only.

### Addressing and Storage

A persisted value is identified by:

1. The owner's navigation path relative to its layout manager.
2. The `layoutId` supplied during registration.

Different view models may use the same layout ID because their navigation paths distinguish the
entries. Within one view model, every active registration must have a unique ID. Registering the same
ID twice on one controller throws `InvalidOperationException`.

The standard shell and pages use `JsonTokenLayoutStore`. Their files are created under the application's
content root:

```text
data/layout/shell/layout.json
data/layout/<page-navigation-id>/layout.json
```

Page navigation IDs are escaped before being used as directory names. Each JSON snapshot contains the
relative navigation path, layout ID, and serialized data. Changing a view model's navigation ID or a
registration's layout ID gives the value a new address; existing data is not migrated automatically,
so keep both IDs stable between releases unless resetting the stored state is intentional.

The default store serializes snapshots with Json.NET and writes enum values as strings. Keep layout
DTOs small and backward-compatible. Invalid store data is logged and ignored; the affected load
callback is not invoked.

Layout DTOs are not versioned or migrated automatically. If a DTO shape must change incompatibly,
introduce a new layout ID or provide a custom store that can migrate older snapshots. The standard
store cannot delete one snapshot; removing its `layout.json` while the application is stopped resets
every layout in that store.

## Using Layout Persistence

### ViewModel State

Define a small serializable DTO containing only the state that should be restored:

```C#
public sealed class WorkspacePageLayout
{
    public string? SelectedPanelId { get; set; }
    public bool IsSidebarOpen { get; set; }
}
```

Register the DTO on the view model's `Layout` controller. The trigger should emit when the user changes
the relevant presentation state:

```C#
private readonly Subject<Unit> _layoutChanged = new();
private bool _isRestoringLayout;

private void RegisterLayoutPersistence()
{
    _layoutChanged.DisposeItWith(Disposable);

    Layout
        .Register<WorkspacePageLayout, Unit>(
            nameof(WorkspacePageViewModel),
            LoadLayout,
            SaveLayout,
            _layoutChanged.Where(_ => !_isRestoringLayout)
        )
        .DisposeItWith(Disposable);

    Layout.LoadWhenRootAttached(RootTracking).DisposeItWith(Disposable);
}

private void LoadLayout(WorkspacePageLayout layout)
{
    _isRestoringLayout = true;
    try
    {
        SelectedPanelId = layout.SelectedPanelId;
        IsSidebarOpen = layout.IsSidebarOpen;
    }
    finally
    {
        _isRestoringLayout = false;
    }
}

private WorkspacePageLayout SaveLayout()
{
    return new WorkspacePageLayout
    {
        SelectedPanelId = SelectedPanelId,
        IsSidebarOpen = IsSidebarOpen,
    };
}

private void OnUserChangedLayout()
{
    _layoutChanged.OnNext(Unit.Default);
}
```

Register all values after their dependent properties have been initialized. Keep both the registration
and the root-tracking subscription alive for the lifetime of the view model.

`LoadWhenRootAttached` loads all current registrations when the view model receives a root. It also
loads again after a later reattachment. If no stored value exists, the load callback is not called and
the view model keeps its defaults.

### Avalonia View State

Use `RegisterLayout` for state owned by an Avalonia control rather than its view model. The helper
observes the control's `DataContext` and registers against it whenever it implements `IViewModel`.
Load and save callbacks run on the UI thread.

The following view persists a scroll offset:

```C#
public sealed class ResultsViewLayout
{
    public Vector ScrollOffset { get; set; }
}

public partial class ResultsView : UserControl
{
    private readonly Subject<Unit>? _layoutChanged;
    private readonly IDisposable? _layout;
    private bool _isRestoringLayout;

    public ResultsView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            return;
        }

        _layoutChanged = new Subject<Unit>();
        _layout = this.RegisterLayout<ResultsViewLayout, Unit>(
            nameof(ResultsView),
            LoadLayout,
            SaveLayout,
            _layoutChanged.Where(_ => !_isRestoringLayout)
        );
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layout?.Dispose();
        _layoutChanged?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void LoadLayout(ResultsViewLayout layout)
    {
        _isRestoringLayout = true;
        try
        {
            PART_ScrollViewer.Offset = layout.ScrollOffset;
        }
        finally
        {
            _isRestoringLayout = false;
        }
    }

    private ResultsViewLayout SaveLayout()
    {
        return new ResultsViewLayout { ScrollOffset = PART_ScrollViewer.Offset };
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        _layoutChanged?.OnNext(Unit.Default);
    }
}
```

`RegisterLayout` immediately loads after finding a compatible data context and re-registers whenever
the data context changes. Dispose the returned registration when the view is no longer used.

Use `RegisterLayoutValue` when the stored state is a struct such as `double`, `bool`, or an enum.
Returning `null` from either helper's save callback skips that save.

### Built-in Control Helpers

| Helper                                    | Persisted state                                                           |
|-------------------------------------------|---------------------------------------------------------------------------|
| `RegisterGridColumnPixelWidth`            | The pixel width of one `Grid` column, including splitter changes.         |
| `RegisterWorkspaceLayout(Workspace)`      | Left, right, and bottom workspace panel sizes after its panel is created. |
| `RegisterWorkspaceLayout(WorkspacePanel)` | The same sizes when the caller already has the underlying panel.          |

The grid helper saves only finite, positive widths. Workspace values are validated and clamped to the
panel's minimum dimensions when restored.

```C#
private readonly IDisposable _layout;

public WorkspacePageView()
{
    InitializeComponent();
    _layout = this.RegisterWorkspaceLayout(
        "WorkspacePage.Workspace",
        PART_Workspace
    );
}
```

## Advanced Usage

### Direct Sink Access

The `ILayoutSink<TData>` returned by the base registration API gives explicit load and save control:

```C#
var sidebarLayout = Layout.Register<bool>(
    nameof(IsSidebarOpen),
    value => IsSidebarOpen = value
);

await sidebarLayout.LoadAsync(cancel);
await sidebarLayout.SaveAsync(IsSidebarOpen, cancel);
```

### Custom Stores

For a custom navigation root that does not derive from the standard shell or page classes, create a
manager explicitly:

```C#
LayoutManager = new LayoutManager<IViewModel>(
    this,
    new JsonTokenLayoutStore(storageDirectory, logger)
).DisposeItWith(Disposable);
```

The manager owns and disposes the supplied store. A custom `ILayoutStore` may use another backend, but
must address entries by both `NavPath` and `layoutId`. `NullLayoutStore.Instance` accepts layout
operations without retaining data and is useful in lightweight design-time or test contexts.

## API {collapsible="true" default-state="collapsed"}

### [ViewLayoutMixin](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Tools/ViewLayoutMixin.cs)

Provides layout registration extensions for controllers and Avalonia controls.

| Target              | Method                                                                                                                          | Return Type   | Description                                                          |
|---------------------|---------------------------------------------------------------------------------------------------------------------------------|---------------|----------------------------------------------------------------------|
| `ILayoutController` | `Register<TData, TTrigger>(string layoutId, Action<TData> load, Func<TData?> save, Observable<TTrigger> trigger)`               | `IDisposable` | Registers and saves a reference-type snapshot.                       |
| `ILayoutController` | `Register<TValue, TTrigger>(string layoutId, Action<TValue> load, Func<TValue?> save, Observable<TTrigger> trigger)`            | `IDisposable` | Registers and saves a value-type snapshot.                           |
| `Control`           | `RegisterLayout<TData, TTrigger>(string layoutId, Action<TData> load, Func<TData?> save, Observable<TTrigger> trigger)`         | `IDisposable` | Persists a reference-type snapshot through the current data context. |
| `Control`           | `RegisterLayoutValue<TValue, TTrigger>(string layoutId, Action<TValue> load, Func<TValue?> save, Observable<TTrigger> trigger)` | `IDisposable` | Persists a value-type snapshot through the current data context.     |
| `Control`           | `RegisterGridColumnPixelWidth(string layoutId, Grid grid, int columnIndex)`                                                     | `IDisposable` | Persists one grid column's pixel width.                              |
| `Control`           | `RegisterWorkspaceLayout(string layoutId, Workspace workspace)`                                                                 | `IDisposable` | Persists the generated panel sizes of a workspace.                   |
| `Control`           | `RegisterWorkspaceLayout(string layoutId, WorkspacePanel panel)`                                                                | `IDisposable` | Persists the dimensions of an existing workspace panel.              |

#### `ILayoutController.Register<TData, TTrigger>`

Registers a reference-type DTO and saves a new snapshot when the trigger emits.

| Type Parameter | Constraint     | Description                         |
|----------------|----------------|-------------------------------------|
| `TData`        | `class, new()` | Reference-type snapshot to persist. |
| `TTrigger`     | none           | Type emitted by the save trigger.   |

| Parameter  | Type                   | Description                                   |
|------------|------------------------|-----------------------------------------------|
| `layoutId` | `string`               | Non-empty ID unique within the controller.    |
| `load`     | `Action<TData>`        | Applies a loaded snapshot.                    |
| `save`     | `Func<TData?>`         | Captures a snapshot; `null` skips the save.   |
| `trigger`  | `Observable<TTrigger>` | Emits when the current state should be saved. |

Returns an `IDisposable` owning the registration and trigger subscription. It does not load the value;
call `LoadAllAsync` or `LoadWhenRootAttached` after completing registration. Trigger emissions received
while a previous save is running are dropped.

#### `ILayoutController.Register<TValue, TTrigger>`

Value-type counterpart of the previous overload.

| Type Parameter | Constraint | Description                       |
|----------------|------------|-----------------------------------|
| `TValue`       | `struct`   | Value-type snapshot to persist.   |
| `TTrigger`     | none       | Type emitted by the save trigger. |

| Parameter  | Type                   | Description                                       |
|------------|------------------------|---------------------------------------------------|
| `layoutId` | `string`               | Non-empty ID unique within the controller.        |
| `load`     | `Action<TValue>`       | Applies a loaded value.                           |
| `save`     | `Func<TValue?>`        | Captures a nullable value; `null` skips the save. |
| `trigger`  | `Observable<TTrigger>` | Emits when the current state should be saved.     |

Returns an `IDisposable` owning the registration and trigger subscription. Loading remains explicit,
and overlapping trigger emissions are dropped.

#### `Control.RegisterLayout<TData, TTrigger>`

Registers a reference-type DTO against the controller of the control's current `IViewModel` data
context.

| Type Parameter | Constraint     | Description                         |
|----------------|----------------|-------------------------------------|
| `TData`        | `class, new()` | Reference-type snapshot to persist. |
| `TTrigger`     | none           | Type emitted by the save trigger.   |

| Parameter  | Type                   | Description                                                |
|------------|------------------------|------------------------------------------------------------|
| `layoutId` | `string`               | Non-empty ID unique within each data context.              |
| `load`     | `Action<TData>`        | Applies a loaded snapshot on the UI thread.                |
| `save`     | `Func<TData?>`         | Captures a snapshot on the UI thread; `null` skips saving. |
| `trigger`  | `Observable<TTrigger>` | Emits when the current state should be saved.              |

The method tracks data-context changes and loads immediately after each compatible context is
registered. The returned `IDisposable` stops tracking and disposes the current registration.

#### `Control.RegisterLayoutValue<TValue, TTrigger>`

Value-type version of `RegisterLayout`.

| Type Parameter | Constraint | Description                       |
|----------------|------------|-----------------------------------|
| `TValue`       | `struct`   | Value-type snapshot to persist.   |
| `TTrigger`     | none       | Type emitted by the save trigger. |

| Parameter  | Type                   | Description                                   |
|------------|------------------------|-----------------------------------------------|
| `layoutId` | `string`               | Non-empty ID unique within each data context. |
| `load`     | `Action<TValue>`       | Applies a loaded value on the UI thread.      |
| `save`     | `Func<TValue?>`        | Captures a nullable value on the UI thread.   |
| `trigger`  | `Observable<TTrigger>` | Emits when the current state should be saved. |

Returning `null` from `save` skips the operation. The returned `IDisposable` owns data-context
tracking, the current registration, and its trigger subscription.

#### `Control.RegisterGridColumnPixelWidth`

Persists one grid column's pixel width.

| Parameter     | Type     | Description                                        |
|---------------|----------|----------------------------------------------------|
| `layoutId`    | `string` | ID associated with the column.                     |
| `grid`        | `Grid`   | Grid containing the column and relevant splitters. |
| `columnIndex` | `int`    | Zero-based index in `grid.ColumnDefinitions`.      |

Returns an `IDisposable` owning the registration and event subscriptions. It throws
`ArgumentOutOfRangeException` for an invalid index and saves only finite, positive widths.

#### `Control.RegisterWorkspaceLayout(Workspace)`

Persists the panel sizes of a `Workspace` control.

| Parameter   | Type        | Description                                        |
|-------------|-------------|----------------------------------------------------|
| `layoutId`  | `string`    | ID associated with the workspace.                  |
| `workspace` | `Workspace` | Workspace whose generated panel should be tracked. |

Registers immediately when the generated `WorkspacePanel` is available; otherwise waits for the
workspace's `Loaded` event. The returned `IDisposable` owns the pending event handler or active panel
registration.

#### `Control.RegisterWorkspaceLayout(WorkspacePanel)`

Persists the left, right, and bottom dimensions of an existing workspace panel.

| Parameter  | Type             | Description                               |
|------------|------------------|-------------------------------------------|
| `layoutId` | `string`         | ID associated with the panel.             |
| `panel`    | `WorkspacePanel` | Panel whose dimensions should be tracked. |

Restored dimensions are validated and clamped to the panel's minimum sizes. `WorkspaceChanged`
triggers saving; if all current dimensions are unavailable, the save is skipped. The returned
`IDisposable` removes the event handler and disposes the registration and internal observable.
