# View Model Base

## Overview

[`ViewModelBase`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/ViewModelBase.cs) is the
fundamental abstract class for all view models in the Asv.Avalonia framework. It implements the [
`IViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/IViewModel.cs) interface and
provides core features for property notification, resource management, and logging.

Key Features:

- **Property Change Notifications**: implements [
  `INotifyPropertyChanged`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-10.0).
- **Thread-Safe Disposal**: a robust disposal pattern ensuring resources are released exactly once.
- **Navigation Support**: implements `ISupportId<NavigationId>` to support argument initialization and routing.

> `ViewModelBase` provides the low-level infrastructure and should rarely be inherited from directly in your
> application. It lacks higher-level features like routing or automatic disposal containers.
> {style="warning"}

## Core Components

### NavigationId

The `IViewModel` interface derives from the `ISupportId<NavigationId>` interface, which contains the `Id` property
(of type `NavigationId`). It serves as the unique identifier for the view model instance. It's essential for:

- Identifying: Distinguishing ViewModels within the application.
- Navigation: Providing the path and parameters for routing (especially in `RoutableViewModel`).

You can initialize it with custom arguments using the `InitArgs` method.

### Property Notification

`IViewModel` inherits from [
`INotifyPropertyChanged`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-10.0).

To update properties, use the `SetField` method. It updates the backing field and automatically raises the
`PropertyChanged` event only when the value has actually changed, preventing unnecessary UI updates.

### Disposal Pattern

`ViewModelBase` implements a thread-safe disposal mechanism. While the base class itself holds no heavy resources, it
establishes the pattern for all derived classes:

1. The public `Dispose()` method handles thread safety and prevents multiple disposal calls.
2. Derived classes should override the `Dispose(bool disposing)` method to perform actual cleanup (e.g., unsubscribing
   from events or stopping timers).

## API {collapsible="true" default-state="collapsed"}

### [IViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/IViewModel.cs)

Defines a base contract for all view models in the application.
This interface provides a unique identifier, supports property change notifications,
and ensures proper disposal of resources.

| Property     | Type   | Description                                                       |
|--------------|--------|-------------------------------------------------------------------|
| `IsDisposed` | `bool` | Gets a value indicating whether the view model has been disposed. |

| Method                   | Return Type | Description                                     |
|--------------------------|-------------|-------------------------------------------------|
| `InitArgs(string? args)` | `void`      | Initializes a navigation ID with the arguments. |

### [ViewModelBase: IViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/ViewModelBase.cs)

Represents the base implementation of a view model that provides
property change notifications and a proper disposal mechanism.
This class is designed to be inherited by other view models.

| Property     | Type           | Description                                                       |
|--------------|----------------|-------------------------------------------------------------------|
| `Id`         | `NavigationId` | A unique ViewModel ID.                                            |
| `Logger`     | `ILogger`      | A logger.                                                         |

| Event             | Type                           | Description                                                                                              |
|-------------------|--------------------------------|----------------------------------------------------------------------------------------------------------|
| `PropertyChanged` | `PropertyChangedEventHandler?` | Occurs when a property value changes. Implements `INotifyPropertyChanged` to support UI binding updates. |

| Method                                                    | Return Type | Description                                                                                                                                        |
|-----------------------------------------------------------|-------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| `Dispose()`                                               | `void`      | Releases resources used by the view model. Ensures that the disposal operation is only performed once.                                             |
| `InternalInitArgs(NameValueCollection)`                   | `void`      | Initializes Navigation ID with args.                                                                                                               |
| `Dispose(bool disposing)`                                 | `void`      | Releases managed resources when disposing. This method must be implemented by derived classes to handle resource cleanup.                          |
| `ThrowIfDisposed()`                                       | `void`      | Throws an `ObjectDisposedException` if the view model has already been disposed. This ensures that disposed objects are not accessed unexpectedly. |
| `SetField<T>(ref T field, T value, string? propertyName)` | `bool`      | Sets the field to the specified value and raises the `PropertyChanged` event if the value has changed.                                             |

#### `ViewModelBase.Dispose(bool disposing)`

| Parameter   | Type   | Description                                                                                                           |
|-------------|--------|-----------------------------------------------------------------------------------------------------------------------|
| `disposing` | `bool` | `true` if called from `Dispose` method to release managed resources; otherwise, `false` if called from the finalizer. |

#### `ViewModelBase.SetField<T>`

| Parameter      | Type      | Description                                                                             |
|----------------|-----------|-----------------------------------------------------------------------------------------|
| `field`        | `ref T`   | The backing field reference.                                                            |
| `value`        | `T`       | The new value to set.                                                                   |
| `propertyName` | `string?` | The name of the property that changed. Automatically set by the caller if not provided. |
