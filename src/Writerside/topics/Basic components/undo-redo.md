# Undo and Redo

## Overview

Undo and redo record reversible changes made by view models and keep a separate history for each
application page. A change can come from a reactive property, an observable collection, or a custom
operation.

The undo system records changes rather than commands. Each history entry contains the navigation path
of the view model that produced the change, a change registration ID, and a serialized payload. The
registered undo and redo callbacks define how that payload is applied.

The underlying implementation belongs to the
[`Asv.Modeling` project](https://github.com/asv-soft/asv-common/tree/main/src/Asv.Modeling/Undo)
in the `asv-common` repository. Asv.Avalonia integrates it with its view-model tree, standard pages,
application data directories, menus, and global hot keys. This guide focuses on that
integration.

Asv.Avalonia provides the following integration points:

| Integration point             | Behavior                                                                                 |
|-------------------------------|------------------------------------------------------------------------------------------|
| `IViewModel.Undo`             | Every framework view model exposes a lazily created `IUndoController`.                   |
| `PageViewModel.UndoHistory`   | Collects changes from the page and all descendants into page-level undo and redo stacks. |
| `IPageContext.UndoStore`      | Supplies the store used by the page history.                                             |
| Standard shell integration    | Creates a `JsonUndoHistoryStore` for every page under the application data directory.    |
| `UndoAction` and `RedoAction` | Connect the standard `Ctrl+Z` and `Ctrl+Y` hot keys to the active page history.          |

Applications built on the standard shell and page classes do not create an undo history or store
themselves. They only register undoable state on the relevant view models.

## How It Works

### Lifecycle

Registration, publication, recording, and execution are separate operations:

| Stage        | What happens                                                                                                                                                         |
|--------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Registration | A view model registers a stable `changeId`, undo and redo callbacks, and a change factory on its `Undo` controller. Registration alone does not add a history entry. |
| Mutation     | Application code applies a user-requested change to the current model or view-model state.                                                                           |
| Publication  | The registration sink publishes a payload containing enough data to revert and reapply that change.                                                                  |
| Recording    | The controller raises a bubbling `UndoEvent`; the page history records the sender's relative navigation path, `changeId`, and serialized payload.                    |
| Undo         | The history resolves the original view model by path, loads the payload, invokes its undo callback, and moves the entry to the redo stack.                           |
| Redo         | The history resolves the same registration, invokes its redo callback, and moves the entry back to the undo stack.                                                   |

Publishing a new change clears the page's redo stack. Each publication creates one history entry;
there is no automatic grouping or coalescing of related changes.

Undo and redo wait for pending payload serialization before executing. If navigation, payload loading,
or a callback fails, the entry is returned to its original stack and the exception is propagated.

During an undo or redo callback, the registration being executed suppresses its own publications.
This prevents a normal value callback from recording the value it is currently restoring. Application
code does not need a local `isUndoing` flag for that registration.

### Addressing and Persistence

A history entry is addressed by:

1. The producing view model's navigation path relative to the page.
2. The `changeId` supplied when the undo handler was registered.

Different view models may use the same change ID because their navigation paths distinguish the
entries. Within one view model, every active registration must have a unique ID. Registering the same
ID twice on one controller throws `UndoException`.

The producing view model must be part of the routable page tree. Set its parent, return it from the
parent's `GetChildren()` implementation, and give it a stable `NavId`. Recording can succeed when only
the parent link exists, but undo and redo also need navigation through `GetChildren()` to find the
target again.

The standard shell gives every page a `JsonUndoHistoryStore`. Its files are created under the
application's content root:

```text
data/undo/<page-navigation-id>/undo-stack.jsonl
data/undo/<page-navigation-id>/redo-stack.jsonl
data/undo/<page-navigation-id>/<payload-id>.undo
```

Page navigation IDs are escaped before being used as directory names. Payloads up to 4 KiB are stored
inline in the stack files as Base64; larger payloads are written to separate `.undo` files. Change
payloads supplied by the standard value and collection helpers are serialized with MessagePack.

The page history serializes published payloads in order. When the history is disposed, it waits for
pending payload writes and rewrites both stack files. Recreating the same page loads those files and
restores its previous undo and redo stacks.

Persisted history depends on the page path, descendant view-model paths, registration IDs, and payload
formats remaining compatible. Changing any of them can make an existing entry impossible to replay.
There is no automatic migration or maximum history depth, so use compact payloads and keep identifiers
stable between releases. Removing the page's undo directory while the application is stopped resets
the stored history for that page.

## Using Undo and Redo

### Reactive Properties

Use `Undo.TrackProperty` when undoing a change means assigning the previous value and redoing it means
assigning the new value:

```C#
public class DocumentViewModel : ViewModel
{
    public DocumentViewModel()
        : base("document")
    {
        Title = new BindableReactiveProperty<string>(string.Empty)
            .DisposeItWith(Disposable);

        Undo.TrackProperty(nameof(Title), Title)
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> Title { get; }
}
```

`TrackProperty` observes every value change, including changes made programmatically. Use explicit
publication when only particular user operations should be added to history.

The converter overload stores a different type from the one exposed by the property. This is useful
when a UI type is not suitable for persistence:

```C#
Undo.TrackProperty<GeoPoint, string>(
        nameof(Location),
        Location,
        GeoPoint.Parse,
        value => value.ToString()
    )
    .DisposeItWith(Disposable);
```

`TrackGeoPointProperty` provides this conversion for `GeoPoint` directly.

### Explicit Value Changes

Use `RegisterValue` when the view model controls when a change becomes a history entry or needs custom
apply logic:

```C#
public class DocumentViewModel : ViewModel
{
    private readonly IUndoChangeSink<ValueUndoChange<string>> _titleUndo;

    public DocumentViewModel()
        : base("document")
    {
        Title = string.Empty;
        _titleUndo = Undo
            .RegisterValue<string>(nameof(Title), ApplyTitle, ApplyTitle)
            .DisposeItWith(Disposable);
    }

    public string Title
    {
        get;
        private set => SetField(ref field, value);
    }

    public void Rename(string newTitle)
    {
        var oldTitle = Title;
        if (oldTitle == newTitle)
        {
            return;
        }

        ApplyTitle(newTitle);
        _titleUndo.PublishUpdate(oldTitle, newTitle);
    }

    private void ApplyTitle(string value)
    {
        Title = value;
    }
}
```

The undo callback receives the old value and the redo callback receives the new value. Asynchronous
callback overloads also receive a cancellation token. `PublishUpdate` ignores equal old and new values
for an update operation.

Apply the original operation before publishing it. If applying the operation fails, no history entry
should be created.

### Observable Collections

Use `Undo.Create` to track add, remove, and replace operations on an `ObservableList<T>`:

```C#
public ObservableList<DocumentItem> Items { get; } = [];

private void RegisterUndo()
{
    Undo.Create(nameof(Items), Items)
        .DisposeItWith(Disposable);
}
```

The helper records affected items and their indexes, then reverses or reapplies the collection event.
Move and reset events are not supported. Item values must remain compatible with the MessagePack
payload written by the history store.

### Invoking Undo and Redo

Every `PageViewModel` exposes commands whose execution state follows the corresponding stack:

```xml
<Button Command="{Binding UndoHistory.Undo}" Content="Undo" />
<Button Command="{Binding UndoHistory.Redo}" Content="Redo" />
```

Application code can invoke the asynchronous methods directly when it needs cancellation or explicit
error handling:

```C#
await UndoHistory.UndoAsync(cancel);
await UndoHistory.RedoAsync(cancel);
```

### Suppressing Publication

Use controller-wide suppression when loading state or applying a batch of programmatic updates that
must not appear in undo history:

```C#
using (Undo.SuppressChangePublication())
{
    Title.Value = snapshot.Title;
    Selection.Value = snapshot.Selection;
}
```

`IUndoChangeSink<TChange>.SuppressChangePublication()` limits suppression to one registration. The
controller scope suppresses all registrations owned by that view model.

Undo and redo automatically suppress the sink whose callback is running. Use controller-wide
suppression inside a callback only when it also modifies other tracked registrations and those
secondary changes must not become separate history entries.

## Advanced Usage

### Custom Changes

When a single history entry must describe a domain operation rather than one value or collection
event, define an `IUndoChange` payload and register it through `IUndoController.Register`.

A custom change must:

1. Contain all data required by both callbacks.
2. Implement `Serialize` and `Deserialize`.
3. Have a factory that creates an empty instance for deserialization.
4. Remain backward-compatible with persisted payloads.

Publish one composite change to make a multi-value operation appear as one undo step. Publishing
several value changes creates several independent steps.

```C#
var moveUndo = Undo.Register<MoveItemsChange>(
        "move-items",
        UndoMove,
        RedoMove,
        () => new MoveItemsChange()
    )
    .DisposeItWith(Disposable);

// Apply the operation first, then publish the payload.
MoveItems(sourceIndex, targetIndex, count);
moveUndo.Publish(new MoveItemsChange(sourceIndex, targetIndex, count));
```

### Custom Histories and Stores

`PageViewModel` creates an `UndoHistory<IViewModel>` from the store supplied by `IPageContext`. The
standard shell supplies `JsonUndoHistoryStore`, but a custom page host can provide another
`IUndoHistoryStore` implementation:

```C#
var pageContext = new PageContext(
    navArgs,
    new DatabaseUndoHistoryStore(connection),
    layoutStore
);
```

The history owns and disposes its store. A custom store must preserve both stacks and serialize each
payload by snapshot. `NullUndoHistoryStore.Instance` is the no-op placeholder used by the design-time
page context; use a real store when undo and redo must replay actual changes.
