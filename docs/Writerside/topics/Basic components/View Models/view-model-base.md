# View Model Base

## Overview

[`ViewModelBase`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/ViewModelBase.cs) is the
fundamental abstract class for all view models in the Asv.Avalonia framework. It implements the [
`IViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/IViewModel.cs) interface and
provides core features for property notification, resource management, and logging.

Key Features:

- **Property Change Notifications**: contains implementation of [
  `INotifyPropertyChanged`](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-10.0).
- **Thread-Safe Disposal**: A robust disposal pattern ensuring resources are released exactly once.
- **Navigation Support**: Integration with `NavigationId` for argument initialization and routing.

> `ViewModelBase` provides the low-level infrastructure and should rarely be inherited from directly in your
> application. It lacks higher-level features like routing or automatic disposal containers.
> {style="warning"}

## Core Components

### NavigationId

The `Id` property (of type `NavigationId`) serves as the unique identifier for the ViewModel instance. It is essential
for:

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
