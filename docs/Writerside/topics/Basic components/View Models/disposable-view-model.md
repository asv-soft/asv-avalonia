# Disposable View Model

## Overview

[
`DisposableViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/DisposableViewModel.cs)
extends [`ViewModelBase`](view-model-base.md) by adding essential tools for resource management: a
`CancellationToken` and a `CompositeDisposable` container. It is designed to simplify the cleanup of subscriptions,
timers, and asynchronous operations.

> Just like [`ViewModelBase`](view-model-base.md), you should rarely inherit from this class directly. Most of its
> functionality is already included in higher-level classes like [`RoutableViewModel`](routable-view-model.md) or [
`PageViewModel`](page-view-model.md).
> {style="warning"}

## Core Components

### CompositeDisposable

The `Disposable` property is a thread-safe container for any `IDisposable` objects. Instead of manually disposing of
every field in the `Dispose` method, you can simply "register" them.

- How to use: Use the `.DisposeItWith(Disposable)` extension method on your subscriptions or resources.
- Behavior: When the ViewModel is disposed, the container automatically disposes of all registered resources in the
  order they were added.

### CancellationToken

The `DisposeCancel` property provides a `CancellationToken` that is automatically triggered when the ViewModel starts
its disposal process.

- **Thread-Safety**: The cancellation and disposal of the source are handled in a thread-safe manner.
- **Primary Use Case**: Passing the token to asynchronous tasks, background loops, or observable subscriptions to ensure
  they stop immediately when the ViewModel is destroyed.

### Disposal Logic

`DisposableViewModel` overrides the `Dispose(bool disposing)` method to ensure a clean teardown. The process follows a
specific order:

1. Cancel: The `CancellationToken` is canceled, notifying all linked tasks to stop.
2. Dispose Source: The `CancellationTokenSource` itself is disposed.
3. Dispose Resources: The `CompositeDisposable` container is disposed, which in turn disposes of all registered objects.
