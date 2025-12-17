# Composition View Locator

## Overview

[`CompositionViewLocator`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Tools/CompositionViewLocator.cs) is a component that automatically finds and binds a **View** to its corresponding **ViewModel** (presentation logic). 
It implements the [`IDataTemplate`](https://api-docs.avaloniaui.net/docs/T_Avalonia_Controls_Templates_IDataTemplate) interface for Avalonia UI and uses the [MEF](https://learn.microsoft.com/en-us/dotnet/framework/mef/) to dynamically resolve views from the dependency container.

## How It Works

`CompositionViewLocator` uses a matching algorithm to find the correct view for a given view model:

1. **Direct Type Match** — searches for a view by the ViewModel's full type name
2. **Interface Match** — if no direct match is found, searches for views based on the interfaces implemented by the ViewModel
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

To use `CompositionViewLocator`, add it to your application's data templates in the app initialization `App.axaml.cs` file:

```C#
_container = containerCfg.CreateContainer();

// Register CompositionViewLocator with Avalonia
DataTemplates.Add(new CompositionViewLocator(_container));
```

## ExportViewFor Attribute

Use the `[ExportViewFor]` attribute to register a view for a view model in the composition container.

**Generic version:**

```C#
[ExportViewFor<MyFeatureViewModel>]
public partial class MyFeatureView : UserControl
{
    public MyFeatureView()
    {
        InitializeComponent();
    }
}
```

**Non-generic version:**

```C#
[ExportViewFor(typeof(MyFeatureViewModel))]
public partial class MyFeatureView : UserControl
{
    public MyFeatureView()
    {
        InitializeComponent();
    }
}
```

## Example

### Basic View Registration

```C#
// ViewModel
public class SearchBoxViewModel : RoutableViewModel
{
    public string SearchText { get; set; }
}

// View
[ExportViewFor<SearchBoxViewModel>]
public partial class SearchBoxView : UserControl
{
    public SearchBoxView()
    {
        InitializeComponent();
    }
}
```

## Troubleshooting

### View Not Found

If a view cannot be found, a `TextBlock` with the view model's type name will be displayed.
This helps identify missing view registrations during development.

Make sure to:

- Decorate your view class with `[ExportViewFor<...>]` or `[ExportViewFor(typeof(...))]`
- Verify the composition container is properly initialized
