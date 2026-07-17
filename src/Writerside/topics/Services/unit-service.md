# Unit Service

## Overview

`IUnitService` is the registry of measurement units used across the application. It lets the app store every
numeric value in the International System of Units (SI) internally, while displaying and parsing it in whatever
unit the user prefers (meters vs. nautical miles, °C vs. K, and so on).

The service is built from two abstractions:

* **`IUnit`** — a physical *quantity* (for example *Distance*, *Temperature*, *Frequency*). It exposes the set of
  available units for that quantity, the SI unit, and the user's currently selected unit.
* **`IUnitItem`** — a concrete *unit of measure* within a quantity (for example *meter* or *nautical mile*).
  It knows how to convert to and from SI, and how to parse, validate and format values.

## Concept

Each `IUnit` groups several `IUnitItem`s. One of them is the SI unit (`InternationalSystemUnit`), and one is the
user's current choice (`CurrentUnitItem`, persisted per quantity). Values flow through the service in SI and are
converted only at the edges — when shown to or entered by the user:

```C#
IUnit? distance = unitService[DistanceUnit.Id];
IUnitItem item = distance.CurrentUnitItem.Value;

// Format an SI value (meters) using the user's selected unit:
string text = item.PrintFromSiWithUnits(1234.5);  // e.g. "1234.5 m" or "0.67 NM"

// Parse user input back into SI:
double si = item.ParseToSi("0.67");
```

Many quantities also ship convenience extension methods, so common lookups stay short:

```C#
IUnitItem? current = unitService.Distance();               // current display unit for Distance
IUnitItem? meters  = unitService[DistanceUnit.Id, DistanceMeterUnitItem.Id];
```

## Built-in Quantities

The default registration covers a wide range of quantities, including Altitude, Angle, Bearing, Distance,
Velocity, Frequency, Temperature, Voltage, Amperage, Power, Capacity, Data Rate, Data Size, Latitude, Longitude,
Time Span and more.

## Defining a Custom Quantity

A quantity is a class deriving from `UnitBase<TConfig>`; its units derive from `UnitItemBase`. `UnitItemBase`
accepts a multiplier that converts an SI value to the displayed unit. `FromSi` multiplies by this value, while
`ToSi` divides by it. The SI unit uses `1.0`:

```C#
public sealed class VolumeConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class VolumeUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(VolumeUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<VolumeConfig>(cfgSvc, items)
{
    public const string Id = "volume";
    public override MaterialIconKind Icon => MaterialIconKind.Ruler;
    public override string Name => "Volume";
    public override string Description => "Volume units";
    public override string UnitId => Id;
}

public sealed class VolumeCubicMeterUnitItem() : UnitItemBase(1.0)
{
    public const string Id = $"{VolumeUnit.Id}.cubic-meter";
    public override string UnitItemId => Id;
    public override string Name => "Cubic meter";
    public override string Symbol => "m³";
    public override string Description => "Volume in cubic meters";
    public override bool IsInternationalSystemUnit => true;
}

public sealed class VolumeLiterUnitItem() : UnitItemBase(1000.0)
{
    public const string Id = $"{VolumeUnit.Id}.liter";
    public override string UnitItemId => Id;
    public override string Name => "Liter";
    public override string Symbol => "L";
    public override string Description => "Volume in liters";
    public override bool IsInternationalSystemUnit => false;
}
```

## Registration

The service and the built-in quantities are registered by the core services, so in a normal app you do not
register them yourself. A module that adds its own quantity registers it directly through the units builder —
the service and the defaults are already in place. Items are keyed by the quantity id, so a unit resolves only
the items that belong to it:

```C#
builder.Units
    .RegisterUnit<VolumeUnit>(VolumeUnit.Id)
    .RegisterItem<VolumeCubicMeterUnitItem>()
    .RegisterItem<VolumeLiterUnitItem>();
```

If you compose the service registration yourself, pass a `configure` delegate to `RegisterUnitService` and
call `RegisterDefault()` inside it to keep the built-in quantities:

```C#
services.RegisterUnitService(units =>
{
    units.RegisterDefault();
    units.RegisterUnit<VolumeUnit>(VolumeUnit.Id)
         .RegisterItem<VolumeCubicMeterUnitItem>()
         .RegisterItem<VolumeLiterUnitItem>();
});
```

> Calling `RegisterUnitService()` without a delegate registers the default set of quantities automatically.
> {style="note"}

## API {collapsible="true" default-state="collapsed"}

### [IUnitService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Units/IUnitService.cs)

Represents a registry of all registered measurement quantities.

| Property | Type                                 | Description                                         |
|----------|--------------------------------------|-----------------------------------------------------|
| `Units`  | `IReadOnlyDictionary<string, IUnit>` | All registered quantities, keyed by their `UnitId`. |

| Indexer                                  | Type         | Description                                             |
|------------------------------------------|--------------|---------------------------------------------------------|
| `this[string unit]`                      | `IUnit?`     | Resolves a quantity by id, or `null` if not registered. |
| `this[string unit, string item]`         | `IUnitItem?` | Resolves a specific unit item within a quantity.        |

| Method                                        | Return Type | Description                                                                          |
|-----------------------------------------------|-------------|--------------------------------------------------------------------------------------|
| `GetRequiredUnitOfType<TUnit>(string unitId)` | `TUnit`     | Resolves a quantity and casts it to `TUnit`; throws if not found or of another type. |

#### `IUnitService.this[string unit]`

| Parameter | Type     | Description              |
|-----------|----------|--------------------------|
| `unit`    | `string` | The quantity identifier. |

#### `IUnitService.this[string unit, string item]`

| Parameter | Type     | Description               |
|-----------|----------|---------------------------|
| `unit`    | `string` | The quantity identifier.  |
| `item`    | `string` | The unit item identifier. |

#### `IUnitService.GetRequiredUnitOfType<TUnit>`

| Parameter | Type     | Description              |
|-----------|----------|--------------------------|
| `unitId`  | `string` | The quantity identifier. |

### [IUnit](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Units/IUnit.cs)

Represents a physical quantity that groups several units of measure.

| Property                  | Type                                     | Description                                                    |
|---------------------------|------------------------------------------|----------------------------------------------------------------|
| `Name`                    | `string`                                 | Display name of the quantity.                                  |
| `Description`             | `string`                                 | Description of the quantity.                                   |
| `UnitId`                  | `string`                                 | Unique identifier of the quantity.                             |
| `AvailableUnits`          | `IReadOnlyDictionary<string, IUnitItem>` | Units of measure available for this quantity.                  |
| `CurrentUnitItem`         | `BindableReactiveProperty<IUnitItem>`    | The user's currently selected and persisted unit of measure.   |
| `InternationalSystemUnit` | `IUnitItem`                              | The SI unit for this quantity.                                 |
| `Icon`                    | `MaterialIconKind`                       | Icon associated with the quantity.                             |

| Indexer                   | Type        | Description                                    |
|---------------------------|-------------|------------------------------------------------|
| `this[string unitItemId]` | `IUnitItem` | Resolves an item, falling back to the SI unit. |

#### `IUnit.this[string unitItemId]`

| Parameter    | Type     | Description               |
|--------------|----------|---------------------------|
| `unitItemId` | `string` | The unit item identifier. |

### [IUnitItem](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Units/IUnitItem.cs)

Represents a concrete unit of measure that handles conversion, parsing, validation and formatting.

| Property                    | Type     | Description                                       |
|-----------------------------|----------|---------------------------------------------------|
| `UnitItemId`                | `string` | Unique identifier of the unit item.               |
| `Name`                      | `string` | Display name of the unit item.                    |
| `Description`               | `string` | Description of the unit item.                     |
| `Symbol`                    | `string` | Symbol appended to formatted values.              |
| `IsInternationalSystemUnit` | `bool`   | Whether this item is the SI unit of its quantity. |

| Method                                               | Return Type        | Description                                                     |
|------------------------------------------------------|--------------------|-----------------------------------------------------------------|
| `IsValid(string? value)`                             | `bool`             | Determines whether text is a valid value in this unit.          |
| `ValidateValue(string? value)`                       | `ValidationResult` | Validates text as a value in this unit.                         |
| `Parse(string? value)`                               | `double`           | Parses text as a value in this unit.                            |
| `ParseToSi(string? value)`                           | `double`           | Parses text as a value in this unit and converts it to SI.      |
| `Print(double value, string? format)`                | `string`           | Formats a value expressed in this unit.                         |
| `PrintFromSi(double value, string? format)`          | `string`           | Converts an SI value to this unit and formats it.               |
| `PrintWithUnits(double value, string? format)`       | `string`           | Formats a value and appends its unit symbol.                    |
| `PrintFromSiWithUnits(double value, string? format)` | `string`           | Converts and formats an SI value, then appends its unit symbol. |
| `FromSi(double siValue)`                             | `double`           | Converts a value from SI to this unit.                          |
| `ToSi(double value)`                                 | `double`           | Converts a value from this unit to SI.                          |

#### `IUnitItem.IsValid`

| Parameter | Type      | Description           |
|-----------|-----------|-----------------------|
| `value`   | `string?` | The text to validate. |

#### `IUnitItem.ValidateValue`

| Parameter | Type      | Description           |
|-----------|-----------|-----------------------|
| `value`   | `string?` | The text to validate. |

#### `IUnitItem.Parse`

| Parameter | Type      | Description        |
|-----------|-----------|--------------------|
| `value`   | `string?` | The text to parse. |

#### `IUnitItem.ParseToSi`

| Parameter | Type      | Description        |
|-----------|-----------|--------------------|
| `value`   | `string?` | The text to parse. |

#### `IUnitItem.Print`

| Parameter | Type      | Description                                                     |
|-----------|-----------|-----------------------------------------------------------------|
| `value`   | `double`  | The value expressed in this unit.                               |
| `format`  | `string?` | The numeric format string, or `null` for the default format.    |

#### `IUnitItem.PrintFromSi`

| Parameter | Type      | Description                                                     |
|-----------|-----------|-----------------------------------------------------------------|
| `value`   | `double`  | The value expressed in SI.                                      |
| `format`  | `string?` | The numeric format string, or `null` for the default format.    |

#### `IUnitItem.PrintWithUnits`

| Parameter | Type      | Description                                                     |
|-----------|-----------|-----------------------------------------------------------------|
| `value`   | `double`  | The value expressed in this unit.                               |
| `format`  | `string?` | The numeric format string, or `null` for the default format.    |

#### `IUnitItem.PrintFromSiWithUnits`

| Parameter | Type      | Description                                                     |
|-----------|-----------|-----------------------------------------------------------------|
| `value`   | `double`  | The value expressed in SI.                                      |
| `format`  | `string?` | The numeric format string, or `null` for the default format.    |

#### `IUnitItem.FromSi`

| Parameter | Type     | Description                |
|-----------|----------|----------------------------|
| `siValue` | `double` | The value expressed in SI. |

#### `IUnitItem.ToSi`

| Parameter | Type     | Description                       |
|-----------|----------|-----------------------------------|
| `value`   | `double` | The value expressed in this unit. |
