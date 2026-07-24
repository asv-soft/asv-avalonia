# Search Box

## Overview

`SearchBoxViewModel` and `SearchBoxView` form a reusable search control for Asv.Avalonia applications. The view displays
a text input, a refresh or cancel button, and a progress indicator. The view model runs an application-provided
`SearchDelegate` whenever the text changes.

The component provides:

- optional debouncing for text changes;
- cancellation of a running search when a new query starts;
- determinate and indeterminate progress indication;
- explicit refresh and clear operations.

`SearchBoxViewModel` does not search a collection by itself. The callback supplied by the application decides how the
query is applied, how progress is reported, and how cancellation is handled.

## Usage

Create the synchronized collection view and pass its filtering callback to `SearchBoxViewModel`:

```C#
public class ItemsPageViewModel : ViewModel
{
    private readonly ISynchronizedView<string, string> _view;

    public ItemsPageViewModel(
        ObservableList<string> source,
        ILoggerFactory loggerFactory
    )
        : base("items_page")
    {
        _view = source.CreateView(item => item).DisposeItWith(Disposable);
        Items = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            ApplySearch,
            TimeSpan.FromMilliseconds(300)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public INotifyCollectionChangedSynchronizedViewList<string> Items { get; }
    public SearchBoxViewModel Search { get; }

    private Task ApplySearch(
        string? text,
        IProgress<double> progress,
        CancellationToken cancel
    )
    {
        progress.Report(0);
        cancel.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(text))
        {
            _view.ResetFilter();
        }
        else
        {
            _view.AttachFilter(item => item.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        progress.Report(1);
        return Task.CompletedTask;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Search;
    }
}
```

Add the view directly to XAML and bind its data context to the view model property:

```xml
<UserControl
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:asv="clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia">

    <asv:SearchBoxView
        DataContext="{Binding Search}"
        Margin="5" />
</UserControl>
```

The initial `Text` value does not run the callback. Call `Refresh()` after initialization when the initial contents of
the target view must be loaded through the same callback:

```C#
Search.Refresh();
```

## Search Behavior

When `throttleTime` is set, the component waits for that interval of inactivity before invoking the callback. Without
it, text changes start the search immediately.

The callback receives three values:

- `text` is the current query.
- `progress` updates the progress bar. Report values from `0` to `1` for determinate progress. Until a value is
  reported, the control uses `double.NaN` and shows an indeterminate progress bar.
- `cancel` is canceled when the user presses the cancel button, when a new query replaces a running query, or when the
  caller's cancellation token is canceled.

`Query(...)` and `Refresh(...)` bypass the debounce interval and start the callback immediately.

## API {collapsible="true" default-state="collapsed"}

### [SearchDelegate](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/SearchBox/SearchBoxViewModel.cs)

Represents the asynchronous callback that applies a search query.

| Delegate                                                                             | Return Type | Description                                                  |
|--------------------------------------------------------------------------------------|-------------|--------------------------------------------------------------|
| `SearchDelegate(string? text, IProgress<double> progress, CancellationToken cancel)` | `Task`      | A task that completes when the search operation finishes.    |

#### `SearchDelegate` parameters

| Parameter  | Type                | Description                                                   |
|------------|---------------------|---------------------------------------------------------------|
| `text`     | `string?`           | The query text supplied by the search box.                    |
| `progress` | `IProgress<double>` | Reports determinate progress from `0` to `1`.                 |
| `cancel`   | `CancellationToken` | A token that is canceled when the current search should stop. |

### [SearchBoxViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/SearchBox/SearchBoxViewModel.cs)

Provides search input state and invokes an application-provided `SearchDelegate` when the query changes.

#### `SearchBoxViewModel` constructors

| Constructor                                                                                                                     | Description                                                                                                            |
|---------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------|
| `SearchBoxViewModel()`                                                                                                          | Initializes a new instance of the `SearchBoxViewModel` class for design-time use. Throws outside Avalonia design mode. |
| `SearchBoxViewModel(string typeId, ILoggerFactory loggerFactory, SearchDelegate searchCallback, TimeSpan? throttleTime = null)` | Initializes a new instance of the `SearchBoxViewModel` class.                                                          |

#### `SearchBoxViewModel(string typeId, ILoggerFactory loggerFactory, SearchDelegate searchCallback, TimeSpan? throttleTime = null)`

| Parameter        | Type             | Description                                                                                      |
|------------------|------------------|--------------------------------------------------------------------------------------------------|
| `typeId`         | `string`         | The type identifier used to build the navigation id.                                             |
| `loggerFactory`  | `ILoggerFactory` | The logger factory used to report callback failures.                                             |
| `searchCallback` | `SearchDelegate` | The callback that applies each search query.                                                     |
| `throttleTime`   | `TimeSpan?`      | The optional debounce interval for text changes. When `null`, changes are processed immediately. |

| Property         | Type                                        | Description                                                                      |
|------------------|---------------------------------------------|----------------------------------------------------------------------------------|
| `Text`           | `HistoricalStringProperty`                  | Gets the historical property that stores the current query text.                 |
| `CanExecute`     | `IReadOnlyBindableReactiveProperty<bool>`   | Gets the idle-state flag, which is `false` while the callback is running.        |
| `IsExecuting`    | `IReadOnlyBindableReactiveProperty<bool>`   | Gets a value indicating whether the search callback is currently running.        |
| `Progress`       | `IReadOnlyBindableReactiveProperty<double>` | Gets the current progress value. `double.NaN` represents indeterminate progress. |
| `RefreshCommand` | `ReactiveCommand`                           | Gets the command that reruns the current query.                                  |

| Method                                                   | Return Type               | Description                                                                 |
|----------------------------------------------------------|---------------------------|-----------------------------------------------------------------------------|
| `Cancel()`                                               | `void`                    | Cancels the current search and restores the idle state.                     |
| `Refresh(CancellationToken cancel = default)`            | `ValueTask`               | Starts the current query again without applying the debounce interval.      |
| `Clear()`                                                | `void`                    | Sets the query text to an empty string.                                     |
| `ClearCommandCall(CancellationToken cancel = default)`   | `ValueTask`               | Clears the query unless the supplied token is already canceled.             |
| `GetChildren()`                                          | `IEnumerable<IViewModel>` | Returns the historical query property as the child view model.              |
| `Navigate(NavId id, CancellationToken cancel = default)` | `ValueTask<IViewModel>`   | Focuses the search input and continues normal child navigation.             |
| `Query(string? text)`                                    | `void`                    | Starts the specified query immediately.                                     |
| `Query(string? text, CancellationToken cancel)`          | `void`                    | Starts the specified query immediately with caller-controlled cancellation. |
| `Focus()`                                                | `void`                    | Requests focus for the query text and its bound input.                      |
| `Report(double value)`                                   | `void`                    | Updates the current search progress.                                        |

#### `SearchBoxViewModel.Refresh`

| Parameter | Type                | Description                                       |
|-----------|---------------------|---------------------------------------------------|
| `cancel`  | `CancellationToken` | A token linked to the started search operation.   |

#### `SearchBoxViewModel.ClearCommandCall`

| Parameter | Type                | Description                                                        |
|-----------|---------------------|--------------------------------------------------------------------|
| `cancel`  | `CancellationToken` | A token that prevents the clear operation when already canceled.   |

#### `SearchBoxViewModel.Navigate`

| Parameter | Type                | Description                                          |
|-----------|---------------------|------------------------------------------------------|
| `id`      | `NavId`             | The navigation id passed to the base implementation. |
| `cancel`  | `CancellationToken` | A token that cancels navigation.                     |

#### `SearchBoxViewModel.Query(string? text)`

| Parameter | Type      | Description                                                   |
|-----------|-----------|---------------------------------------------------------------|
| `text`    | `string?` | The query to start. `null` is treated as an empty string.     |

#### `SearchBoxViewModel.Query(string? text, CancellationToken cancel)`

| Parameter | Type                | Description                                                   |
|-----------|---------------------|---------------------------------------------------------------|
| `text`    | `string?`           | The query to start. `null` is treated as an empty string.     |
| `cancel`  | `CancellationToken` | A token linked to the started search operation.               |

#### `SearchBoxViewModel.Report`

| Parameter | Type     | Description                                    |
|-----------|----------|------------------------------------------------|
| `value`   | `double` | The progress value exposed through `Progress`. |
