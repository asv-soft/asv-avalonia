# Navigation Service

## Overview

The [`NavigationService`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Services/Navigation/NavigationService.cs) is a central infrastructural component of Asv.Avalonia responsible for page-based navigation and focus management. 
It implements the [`INavigationService`](#inavigationservice) and is designed to work closely with the [`IRoutable`](routable-view-model.md) View Models.

## When to Use

In most standard scenarios, you do not need to interact with Navigation Service directly. The framework handles common navigation tasks via standard commands (like `OpenPageCommandBase`). 
However, direct usage of the Navigation Service is required in the following scenarios:

1. Complex Navigation Logic: When the destination page depends on runtime conditions (e.g., "If login succeeds → Go Home, otherwise → Show Error").
2. Navigation from Non-UI Contexts: Triggering navigation from background services, timers, or external events (e.g., deep linking).
3. Manual History Management: When you need to programmatically trigger Backward or Forward actions outside of standard UI bindings.
4. Focus Management: When you need to force focus on a specific `IRoutable` element (e.g., focusing a specific widget or input field after a validation error).

## Key Features

1. Path-Based Navigation: Navigate to any `ViewModel` hierarchy using `NavigationPath` (similar to URL routing).
2. History Management: Automatically maintains Forward/Backward stacks using `ObservableStack`.
3. Automatic Focus Tracking:
    1. The service listens to global UI events (`GotFocus`, `PointerPressed`) on the window.
    2. When a user clicks a control inside a view, the service automatically finds the corresponding `IRoutable` ViewModel in the data context and updates the `SelectedControl` property.
    3. This ensures the "Active Page" state is always synced with what the user is actually interacting with.

## How it works

### The Shell Relationship

The Navigation Service doesn't "render" views itself. Instead, it delegates the actual navigation logic to the [`IShell`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Shell/IShell.cs). When you call `GoTo`, the service:

1. Validates the path.
2. Delegates the routing to the Shell via `_host.Shell.NavigateByPath()`.
3. Updates the focus to the resulting ViewModel.

### Focus & Selection Logic

This is the most complex part of the service.

1. Manual Navigation: When calling `GoTo`, the service explicitly sets focus to the target ViewModel (if it implements [`ISupportFocus`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Commands/Behaviour/Focus/ISupportFocus.cs)).

2. User Interaction: If the user clicks on a textbox deep inside a page, the service catches this event, walks up the visual/logical tree to find the nearest `IRoutable` parent, and sets it as the `SelectedControl`.

## Usage Examples

### Using Navigation Commands

Many built-in commands use the Navigation Service to open pages:

```C#
[ExportCommand]
[method: ImportingConstructor]
public class OpenSettingsCommand(INavigationService nav)
    : OpenPageCommandBase(SettingsPageViewModel.PageId, nav)
{
    public override ICommandInfo Info => StaticInfo;

    #region Static

    // ID of the Command
    public const string Id = $"{BaseId}.open.settings";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open Settings",
        Description = "Opens Settings page",
        Icon = MaterialIconKind.Settings,
        IconColor = AsvColorKind.None,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
}
```

### Basic Navigation

To navigate to a specific page, you can use the `GoTo` method:

```C#
// Navigate to a page by its ID
await _navigationService.GoTo(new NavigationPath(SettingsPageViewModel.PageId));
```

## API {collapsible="true" default-state="collapsed"}

### [INavigationService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Services/Navigation/INavigationService.cs)

Represents a navigation service for handling application navigation, navigation history, and focus management.

#### Properties

| Property              | Type                                       | Description                                                                  |
|-----------------------|--------------------------------------------|------------------------------------------------------------------------------|
| `BackwardStack`       | `IObservableCollection<NavigationPath>`    | Gets the observable collection representing the backward navigation history. |
| `Backward`            | `ReactiveCommand`                          | Gets the сommand that triggers backward navigation.                          |
| `ForwardStack`        | `IObservableCollection<NavigationPath>`    | Gets the observable collection representing the forward navigation history.  |
| `Forward`             | `ReactiveCommand`                          | Gets the сommand that triggers forward navigation.                           |
| `SelectedControl`     | `ReadOnlyReactiveProperty<IRoutable?>`     | Gets the сurrently selected (focused) routable control.                      |
| `SelectedControlPath` | `ReadOnlyReactiveProperty<NavigationPath>` | Gets the navigation path of the currently selected control.                  |
| `GoHome`              | `ReactiveCommand`                          | Gets the сommand that triggers navigation to the home page.                  |

#### Methods

| Method                            | Return Type            | Description                                                                            |
|-----------------------------------|------------------------|----------------------------------------------------------------------------------------|
| `BackwardAsync()`                 | `ValueTask`            | Navigates to the previous item in the backward navigation stack.                       |
| `ForwardAsync()`                  | `ValueTask`            | Navigates to the next item in the forward navigation stack.                            |
| `GoTo(NavigationPath path)`       | `ValueTask<IRoutable>` | Navigates to the specified navigation path and returns the resulting routable control. |
| `GoHomeAsync()`                   | `ValueTask`            | Navigates to the home page.                                                            |
| `ForceFocus(IRoutable? routable)` | `void`                 | Forces focus change to the specified routable control.                                 |

#### `INavigationService.GoTo`

| Parameter | Type             | Description          |
|-----------|------------------|----------------------|
| `path`    | `NavigationPath` | Path to navigate to. |

#### `INavigationService.ForceFocus`

| Parameter  | Type         | Description                                      |
|------------|--------------|--------------------------------------------------|
| `routable` | `IRoutable?` | Routable control to be set as currently focused. |
