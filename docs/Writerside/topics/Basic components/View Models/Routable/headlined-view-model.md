# Headlined View Model

## Overview

[
`HeadlinedViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Headlined/HeadlinedViewModel.cs)
is a view model that extends [`RoutableViewModel`](routable-view-model.md) and adds properties for displaying a title (
header), icon, and description.
It is designed for view models such as menus, tabs, toolbars, or any other component that requires a headline with an
icon.

## Core Components

`HeadlinedViewModel` implements the [
`IHeadlinedViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Headlined/IHeadlinedViewModel.cs)
interface, which provides the following properties:

```C#
public interface IHeadlinedViewModel : IRoutable
{
    MaterialIconKind? Icon { get; set; }
    
    AsvColorKind IconColor { get; set; }

    string? Header { get; set; }

    string? Description { get; set; }

    bool IsVisible { get; set; }

    int Order { get; set; }
}
```

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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
```
