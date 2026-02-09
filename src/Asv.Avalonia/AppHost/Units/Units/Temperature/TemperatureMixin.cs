namespace Asv.Avalonia;

public static class TemperatureMixin
{
    public static UnitsBuilder RegisterTemperature(this UnitsBuilder builder)
    {
        builder
            .AddUnit<TemperatureUnit>(TemperatureUnit.Id)
            .AddItem<TemperatureCelsiusUnitItem>()
            .AddItem<TemperatureFahrenheitUnitItem>()
            .AddItem<TemperatureKelvinUnitItem>();
        return builder;
    }

    public static IUnitItem? Temperature(this IUnitService service) =>
        service[TemperatureUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? TemperatureCelsius(this IUnitService service) =>
        service[TemperatureUnit.Id, TemperatureCelsiusUnitItem.Id];

    public static IUnitItem? TemperatureFahrenheit(this IUnitService service) =>
        service[TemperatureUnit.Id, TemperatureFahrenheitUnitItem.Id];

    public static IUnitItem? TemperatureKelvin(this IUnitService service) =>
        service[TemperatureUnit.Id, TemperatureKelvinUnitItem.Id];
}
