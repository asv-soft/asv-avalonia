# Hot Key Service

## Overview

`IHotKeyService` is the central component responsible for global keyboard shortcuts.
It listens for key presses on the application window, matches them against the set of registered
hot key actions, and executes the matching action against the currently focused view model.

Every shortcut is described by an `IHotKeyAction`. The service also persists user overrides:
when a user rebinds an action, the new gesture is saved to the application configuration;
default gestures are not stored, so resetting a binding simply removes the override.

## Key Features

1. **Global Dispatch:** Subscribes to `KeyDown` on the current `TopLevel` and routes each gesture to a matching action.
2. **Context Awareness:** An action is evaluated against the currently focused view model (`Shell.Navigation.SelectedControl`).
   A typed action (`HotKeyAction<T>`) runs when that view model — or one of its routable ancestors — is of type `T`; otherwise it is skipped.
3. **Rebinding & Persistence:** Gestures can be changed at runtime through the indexer. Overrides are stored in
   `HotKeyServiceConfig`; assigning the default gesture (or `null`) removes the override.
4. **CanExecute Tracking:** `ObserveCanExecute` reports whether an action can currently run — convenient for
   enabling or disabling UI elements.

## Hot Key Actions

A shortcut is defined by implementing `IHotKeyAction` (which extends `IHotKeyInfo`).
The framework provides the `HotKeyAction<T>` base class that targets a capability interface `T`: the action can execute
when the focused view model — or any of its ancestors in the routable tree — is of type `T`, and `InternalExecute`
receives that matched instance already cast.

```C#
public class SaveAction : HotKeyAction<ISupportSave>
{
    public const string Id = "save";
    public override string ActionId => Id;
    public override string Name => RS.SaveCommand_CommandInfo_Name;
    public override string Description => RS.SaveCommand_CommandInfo_Description;
    public override MaterialIconKind Icon => MaterialIconKind.FloppyDisc;
    public override KeyGesture DefaultHotKey => new(Key.S, KeyModifiers.Control);

    protected override ValueTask InternalExecute(ISupportSave target, CancellationToken cancel)
    {
        return target.Save();
    }
}
```

The framework registers a default set of actions out of the box, including `SaveAction`, `SaveAsAction`,
`UndoAction`, `RedoAction`, `SearchAction`, `RefreshAction`, `ClosePageAction`, `OpenFileAction`,
`OpenHomePageAction`, `CancelAction` and `ClearAction`.

## Registration

The service and its default actions are registered by the core services, so in a normal app you do not
register them yourself. A module that needs an extra shortcut just adds its own action — the service and
the built-in actions are already in place:

```C#
builder.HotKeys.Register<MyCustomAction>();
```

If you compose the service registration yourself, pass a `configure` delegate to `RegisterHotKeys` and
call `RegisterDefault()` inside it to keep the built-in actions:

```C#
services.RegisterHotKeys(hotKeys =>
{
    hotKeys.RegisterDefault();          // built-in actions
    hotKeys.Register<MyCustomAction>(); // your own
});
```

> Calling `RegisterHotKeys()` without a delegate registers the default set of actions automatically.
> {style="note"}

## Rebinding a Hot Key

Use the indexer to read or change the gesture bound to an action. Assigning `null` restores the default:

```C#
// Read the current gesture
var gesture = hotKeyService[SaveAction.Id];

// Rebind
hotKeyService[SaveAction.Id] = new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Shift);

// Restore the default
hotKeyService[SaveAction.Id] = null;
```

## API {collapsible="true" default-state="collapsed"}

### [IHotKeyService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/HotKeys/IHotKeyService.cs)

Dispatches global keyboard shortcuts and stores user overrides.

| Property                 | Type                                                   | Description                                                          |
|--------------------------|--------------------------------------------------------|----------------------------------------------------------------------|
| `OnHotKey`               | `Observable<KeyGesture>`                               | Emits every recognized gesture before it is dispatched to an action. |
| `IsHotKeyEnabled`        | `bool`                                                 | Enables or disables global hot key handling.                         |
| `Actions`                | `IEnumerable<IHotKeyInfo>`                             | All registered actions.                                              |
| `OnHotKeyGestureChanged` | `Observable<(IHotKeyInfo Action, KeyGesture Gesture)>` | Emits when an action's gesture changes.                              |

| Indexer                 | Type          | Description                                                                                  |
|-------------------------|---------------|----------------------------------------------------------------------------------------------|
| `this[string hotKeyId]` | `KeyGesture?` | Gets or sets the gesture bound to an action id. Setting `null` restores the default gesture. |

| Method                               | Return Type        | Description                                                     |
|--------------------------------------|--------------------|-----------------------------------------------------------------|
| `ObserveCanExecute(string actionId)` | `Observable<bool>` | Reports whether the action can execute for the current context. |

#### `IHotKeyService.this[string hotKeyId]`

| Parameter  | Type     | Description            |
|------------|----------|------------------------|
| `hotKeyId` | `string` | The action identifier. |

#### `IHotKeyService.ObserveCanExecute`

| Parameter  | Type     | Description            |
|------------|----------|------------------------|
| `actionId` | `string` | The action identifier. |

### [IHotKeyInfo](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/HotKeys/IHotKeyInfo.cs)

Provides a read-only description of a hot key action.

| Property        | Type               | Description                               |
|-----------------|--------------------|-------------------------------------------|
| `ActionId`      | `string`           | Unique identifier of the action.          |
| `Name`          | `string`           | Display name.                             |
| `Description`   | `string`           | Human-readable description.               |
| `Icon`          | `MaterialIconKind` | Icon associated with the action.          |
| `DefaultHotKey` | `KeyGesture`       | Default gesture used when not overridden. |

### [IHotKeyAction](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/HotKeys/IHotKeyAction.cs)

Defines an executable hot key action. Extends `IHotKeyInfo`.

| Method                                                     | Return Type | Description                                                  |
|------------------------------------------------------------|-------------|--------------------------------------------------------------|
| `CanExecute(IViewModel context)`                           | `bool`      | Whether the action can run for the given context.            |
| `Execute(IViewModel context, CancellationToken cancel)`    | `ValueTask` | Runs the action against the given context.                   |

#### `IHotKeyAction.CanExecute`

| Parameter | Type         | Description             |
|-----------|--------------|-------------------------|
| `context` | `IViewModel` | The view model context. |

#### `IHotKeyAction.Execute`

| Parameter | Type                | Description                             |
|-----------|---------------------|-----------------------------------------|
| `context` | `IViewModel`        | The view model context.                 |
| `cancel`  | `CancellationToken` | A token that cancels the operation.     |
