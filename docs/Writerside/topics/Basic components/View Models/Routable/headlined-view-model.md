# Headlined View Model

## Overview

[`HeadlinedViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Headlined/HeadlinedViewModel.cs)
is a view model that extends [`RoutableViewModel`](routable-view-model.md) and adds properties for displaying a 
title (header), icon, and description.
It is designed for view models such as menus, tabs, toolbars, or any other components that require a headline with an icon.

## Common Use Cases

### Menu Items

Example of an item view model for a menu:

```C#
public class MenuItemViewModel : HeadlinedViewModel
{
    public MenuItemViewModel(string header, NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Header = header;
        Icon = MaterialIconKind.Cog;
        IconColor = AsvColorKind.Success;
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
```

## API {collapsible="true" default-state="collapsed"}

### [HeadlinedViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Headlined/HeadlinedViewModel.cs)

Represents a base view model with a title (header) and an optional icon.
This can be used as a foundation for view models that require a title and icon representation.

| Property      | Type                | Description                                           |
|---------------|---------------------|-------------------------------------------------------|
| `Icon`        | `MaterialIconKind?` | Gets or sets the icon associated with the view model. |
| `IconColor`   | `AsvColorKind`      | Gets or sets the brush for the icon.                  |
| `IsVisible`   | `bool`              | Gets or sets the item visibility.                     |
| `Header`      | `string?`           | Gets or sets the header (title) of the view model.    |
| `Description` | `string?`           | Gets or sets the description of the view model.       |
| `Order`       | `int`               | Gets or sets the order of the view model.             |
