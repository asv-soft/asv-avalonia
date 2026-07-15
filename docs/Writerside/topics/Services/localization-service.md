# Localization Service

## Overview

`ILocalizationService` manages the application's UI language. It exposes the available languages and
the currently selected one, applies the matching `CultureInfo` to the UI culture, and persists the
choice so it survives a restart — falling back to English when nothing is stored yet, or when the
stored id matches no available language.

The translated strings themselves live in `.resx` resources — `RS.resx` for the neutral (English) set
and `RS.ru.resx` for Russian — and are read through the generated `RS` class. The service does not
translate anything on its own: it only selects the culture that the resource lookup resolves against.
Only the UI culture is affected — `CultureInfo.CurrentCulture` is left untouched, so number, date and
currency formatting do not follow the selected language.

## Available Languages

The default `LocalizationService` ships these two languages:

| Id   | Display name |
|------|--------------|
| `en` | English (EN) |
| `ru` | Русский (RU) |

The list is hardcoded in that implementation — there is no DI hook to extend it.

## Usage

Read or change the current language through `CurrentLanguage.Value`:

```C#
// Switch to Russian
var russian = localizationService.AvailableLanguages.First(x => x.Id == "ru");
localizationService.CurrentLanguage.Value = russian;
```

To react to changes, subscribe to the property. `SynchronizedReactiveProperty<T>` pushes its current
value on subscribe, so use `Skip(1)` when you only care about subsequent changes:

```C#
localizationService.CurrentLanguage
    .Skip(1)
    .ObserveOnUIThreadDispatcher()
    .Subscribe(language => { /* react to the new language */ })
    .DisposeItWith(Disposable);
```

> Switching the culture does not re-translate views that have already been built. The built-in picker
> in **Settings → Appearance** (`LanguageProperty`) therefore asks the user to confirm a restart after
> the language is changed.
> {style="warning"}

## Registration

The service is part of the core services and its registration takes no configuration callback:

```C#
services.RegisterLocalizationService();
```

In a design-time environment the registration substitutes `NullLocalizationService.Instance` instead.

`LocalizationService` derives from `ServiceWithConfigBase<LocalizationServiceConfig>`, which gives it
the load and save plumbing for its configuration — see [What is a Service?](what-is-a-service.md).

## API {collapsible="true" default-state="collapsed"}

### [ILocalizationService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Localization/ILocalizationService.cs)

Manages the application's current UI language and available language choices.

| Property             | Type                                          | Description                                                                                                                                                                                      |
|----------------------|-----------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `CurrentLanguage`    | `SynchronizedReactiveProperty<ILanguageInfo>` | The active language. Assign an instance from `AvailableLanguages`: the culture is applied and the choice persisted. Any other `ILanguageInfo` implementation is rejected and nothing is applied. |
| `AvailableLanguages` | `IEnumerable<ILanguageInfo>`                  | All available languages.                                                                                                                                                                         |

### [ILanguageInfo](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Localization/ILanguageInfo.cs)

Describes an application UI language.

| Property      | Type     | Description                                   |
|---------------|----------|-----------------------------------------------|
| `Id`          | `string` | Culture id of the language (e.g. `en`, `ru`). |
| `DisplayName` | `string` | Human-readable name shown in the UI.          |

### [LanguageInfo](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Localization/LocalizationService.cs)

Represents the default `ILanguageInfo` implementation.

| Property  | Type          | Description                                                               |
|-----------|---------------|---------------------------------------------------------------------------|
| `Culture` | `CultureInfo` | Culture applied when the language is selected; resolved lazily on access. |
