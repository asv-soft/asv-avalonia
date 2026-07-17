# Theme Service

## Overview

`IThemeService` controls the application's visual appearance: which theme is active (Dark or Light)
and whether the UI uses the compact density. Assigning a theme applies the matching Avalonia
[`ThemeVariant`](https://docs.avaloniaui.net/docs/styling/theme-variants) to
`Application.Current.RequestedThemeVariant` and takes effect immediately; compact
mode switches the Fluent theme's `DensityStyle` between `Compact` and `Normal`.

Both values are stored in `ThemeServiceConfig` (theme defaults to `dark`, compact to `false`) and
restored on startup. The service exposes its state as reactive properties, so a view model can read
the current value and react to changes through the same member.

## Available Themes

The default `ThemeService` ships these two themes:

| Id      | Constant                  | Avalonia variant     |
|---------|---------------------------|----------------------|
| `dark`  | `ThemeService.DarkTheme`  | `ThemeVariant.Dark`  |
| `light` | `ThemeService.LightTheme` | `ThemeVariant.Light` |

The list is hardcoded in that implementation — there is no DI hook to extend it.

## Usage

Read or change the current theme through `CurrentTheme.Value`. Always assign one of the items from
`Themes` — any other `IThemeInfo` implementation is rejected and applies nothing:

```C#
// Switch to the light theme
var light = themeService.Themes.First(x => x.Id == ThemeService.LightTheme);
themeService.CurrentTheme.Value = light;

// Turn on compact density
themeService.IsCompact.Value = true;
```

To react to changes, subscribe to the property. `SynchronizedReactiveProperty<T>` pushes its current
value on subscribe, so use `Skip(1)` when you only care about subsequent changes:

```C#
themeService.CurrentTheme
    .Skip(1)
    .ObserveOnUIThreadDispatcher()
    .Subscribe(theme => { /* react to the new theme */ })
    .DisposeItWith(Disposable);
```

> Users pick the theme in **Settings → Appearance**. See `ThemeProperty` for the built-in picker.
> {style="note"}

## Registration

The service is part of the core services and takes no configuration:

```C#
services.RegisterThemeService();
```

In a design-time environment the registration substitutes `NullThemeService.Instance` instead.

## API {collapsible="true" default-state="collapsed"}

### [IThemeService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Theme/IThemeService.cs)

Manages the application's theme and control density.

| Property       | Type                                       | Description                                                                      |
|----------------|--------------------------------------------|----------------------------------------------------------------------------------|
| `Themes`       | `IEnumerable<IThemeInfo>`                  | All available themes.                                                            |
| `CurrentTheme` | `SynchronizedReactiveProperty<IThemeInfo>` | Gets the active theme selected from `Themes`                                     |
| `IsCompact`    | `SynchronizedReactiveProperty<bool>`       | Whether compact density is on. Assigning it applies the density and persists it. |

### [IThemeInfo](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Theme/IThemeService.cs)

Represents a theme item.

| Property | Type     | Description                              |
|----------|----------|------------------------------------------|
| `Id`     | `string` | Unique id of the theme (e.g. `dark`).    |
| `Name`   | `string` | Localized display name.                  |

### [ThemeItem](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Theme/ThemeItem.cs)

Represents the default `IThemeInfo` implementation.

| Property | Type           | Description                                               |
|----------|----------------|-----------------------------------------------------------|
| `Theme`  | `ThemeVariant` | Avalonia theme variant applied when the theme is selected. |
