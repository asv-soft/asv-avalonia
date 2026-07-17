# Action View Model

## Overview

[`ActionViewModel`](#actionviewmodel) is a view model that extends [`HeadlinedViewModel`](headlined-view-model.md) and
adds action functionality. It is often used for menu items that trigger an `ICommand` — typically an R3
`ReactiveCommand`.

## API {collapsible="true" default-state="collapsed"}

### [ActionViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Action/ActionViewModel.cs)

Represents a view model that supports an actionable command, inheriting header, icon, and description properties.

#### `ActionViewModel` constructor

| Constructor                      | Description                                                                                    |
|----------------------------------|------------------------------------------------------------------------------------------------|
| `ActionViewModel(string typeId)` | Passes the type identifier to the base `HeadlinedViewModel`, which builds the `Id` from it.    |

| Property           | Type        | Description                                                                |
|--------------------|-------------|----------------------------------------------------------------------------|
| `Command`          | `ICommand?` | Gets or sets the action that is executed when the view model is activated. |
| `CommandParameter` | `object?`   | Gets or sets the parameter passed to the command when executed.            |
