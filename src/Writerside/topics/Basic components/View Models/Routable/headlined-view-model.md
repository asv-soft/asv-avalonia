# Headlined View Model

## Overview

[`HeadlinedViewModel`](#headlinedviewmodel) is a view model that extends [`ViewModel`](view-model.md) and implements
`IHeadlinedViewModel`, adding properties for displaying a title (header), icon, and description. It is designed for view
models such as menus, tabs, toolbars, or any other components that require a headline with an icon.

## Common Use Cases

### Menu Items

Example of an item view model for a menu:

```C#
public class MenuItemViewModel : HeadlinedViewModel
{
    public MenuItemViewModel(string typeId, string header)
        : base(typeId)
    {
        Header = header;
        Icon = MaterialIconKind.Cog;
        IconColor = AsvColorKind.Success;
    }
}
```

`HeadlinedViewModel` already overrides `GetChildren()` to return an empty sequence, so a leaf item like this does not
need to override it — do so only when your view model has child view models.

## API {collapsible="true" default-state="collapsed"}

### [HeadlinedViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Headlined/HeadlinedViewModel.cs)

Represents a base view model with a title (header) and an optional icon.
This can be used as a foundation for view models that require a title and icon representation.

#### `HeadlinedViewModel` constructor

| Constructor                         | Description                                                                        |
|-------------------------------------|------------------------------------------------------------------------------------|
| `HeadlinedViewModel(string typeId)` | Passes the type identifier to the base `ViewModel`, which builds the `Id` from it. |

| Property      | Type                | Description                                           |
|---------------|---------------------|-------------------------------------------------------|
| `Icon`        | `MaterialIconKind?` | Gets or sets the icon associated with the view model. |
| `IconColor`   | `AsvColorKind`      | Gets or sets the palette color kind of the icon.      |
| `IsVisible`   | `bool`              | Gets or sets the item visibility.                     |
| `Header`      | `string?`           | Gets or sets the header (title) of the view model.    |
| `Description` | `string?`           | Gets or sets the description of the view model.       |
| `Order`       | `int`               | Gets or sets the order of the view model.             |

| Method          | Return Type               | Description                                                                         |
|-----------------|---------------------------|-------------------------------------------------------------------------------------|
| `GetChildren()` | `IEnumerable<IViewModel>` | Override. Returns an empty sequence, since a headlined view model has no children.  |
