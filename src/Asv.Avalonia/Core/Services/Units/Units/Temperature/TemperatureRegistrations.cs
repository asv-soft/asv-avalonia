namespace Asv.Avalonia;

public static class TemperatureRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterTemperature(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<TemperatureUnit>(TemperatureUnit.Id)
            .RegisterItem<TemperatureCelsiusUnitItem>()
            .RegisterItem<TemperatureFahrenheitUnitItem>()
            .RegisterItem<TemperatureKelvinUnitItem>();
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
