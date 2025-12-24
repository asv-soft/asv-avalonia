# Action View Model

## Overview

[
`ActionViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Action/ActionViewModel.cs)
is a view model that extends [`HeadlinedViewModel`](headlined-view-model.md) and adds action functionality.

It is often used for menu items that trigger an operation when selected.

## Core Components

`ActionViewModel` implements the [
`IActionViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Action/IActionViewModel.cs)
interface, which provides the following properties:

```C#
public interface IActionViewModel : IHeadlinedViewModel
{
    ICommand? Command { get; set; }
    
    object? CommandParameter { get; }
}
```
