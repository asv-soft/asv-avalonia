# Action View Model

## Overview

[`ActionViewModel`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Action/ActionViewModel.cs)
is a view model that extends [`HeadlinedViewModel`](headlined-view-model.md) and adds action functionality. 
It is often used for menu items that trigger [commands](command-service.md).

## API {collapsible="true" default-state="collapsed"}

### [ActionViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/ViewModel/Action/ActionViewModel.cs)

Represents a view model that supports an actionable command, inheriting header, icon, and description properties.

| Property           | Type        | Description                                                                |
|--------------------|-------------|----------------------------------------------------------------------------|
| `Command`          | `ICommand?` | Gets or sets the action that is executed when the view model is activated. |
| `CommandParameter` | `object?`   | Gets the parameter passed to the command when executed.                    |
