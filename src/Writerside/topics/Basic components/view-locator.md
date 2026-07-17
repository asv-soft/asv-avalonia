# View Locator

## Overview

[`ViewLocator`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/ViewLocator/ViewLocator.cs)
is a component that automatically finds and binds a **View** to its corresponding **ViewModel** (presentation logic).
It implements the [`IDataTemplate`](https://api-docs.avaloniaui.net/docs/T_Avalonia_Controls_Templates_IDataTemplate)
interface for Avalonia UI and uses `IServiceProvider` to dynamically resolve views from the dependency container.

## How It Works

`ViewLocator` uses a matching algorithm to find the correct view for a given view model:

1. **Direct Type Match** — searches for a view by the ViewModel's full type name
2.  **Interface Match** — if no direct match is found, searches for views based on the interfaces implemented by the
   ViewModel
3. **Base Class Hierarchy** — if still not found, searches through the base class hierarchy
4. **Fallback** — if no view is found, displays a `TextBlock` with the ViewModel's type name for debugging

The locator only matches the objects implementing `IViewModel`:

```C#
public bool Match(object? data)
{
    return data is IViewModel;
}
```

## Setup

`ViewLocator` is registered in DI by `RegisterViewLocator()`, which is part of the default service set — calling
`RegisterServices()` with no arguments gives you the locator for free. If you pass a custom `configure` action to
`RegisterServices(...)`, the defaults are skipped and you must call `.RegisterViewLocator()` yourself.

`AsvApplication` then resolves that singleton in its constructor and adds it to the application's `DataTemplates`
collection. If the service is missing, it throws `InvalidOperationException`.

## Registering Views

Use the `ViewLocator` builder in your mixin to register a view for a view model:

```C#
builder.ViewLocator.RegisterViewFor<MyFeatureViewModel, MyFeatureView>();
```

This registers the view as a keyed transient service in the `IServiceCollection` container.

## Example

### Basic View Registration

```C#
// ViewModel
public class MyFeatureViewModel : ViewModel
{
    public const string ViewModelId = "my.feature";

    public MyFeatureViewModel()
        : base(ViewModelId) { }

    public string? SearchText { get; set; }
}

// View
public partial class MyFeatureView : UserControl
{
    public MyFeatureView()
    {
        InitializeComponent();
    }
}

// Registration (in your mixin):
builder.ViewLocator.RegisterViewFor<MyFeatureViewModel, MyFeatureView>();
```

## Troubleshooting

### View Not Found

If a view cannot be found, a `TextBlock` with the view model's type name will be displayed.
This helps identify missing view registrations during development.

Make sure to:

- Register the view for your view model via `builder.ViewLocator.RegisterViewFor<TViewModel, TView>()`
- Verify your view is a `Control` subclass
