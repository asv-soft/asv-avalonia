namespace Asv.Avalonia;

public static class VelocityMixin
{
    public static UnitsBuilder RegisterVelocity(this UnitsBuilder builder)
    {
        builder
            .AddUnit<VelocityUnit>(VelocityUnit.Id)
            .AddItem<VelocityMetersPerSecondUnitItem>()
            .AddItem<VelocityKilometersPerHourUnitItem>()
            .AddItem<VelocityMilesPerHourUnitItem>();
        return builder;
    }

    public static IUnitItem? Velocity(this IUnitService service)
    {
        return service[VelocityUnit.Id]?.CurrentUnitItem.Value;
    }

    public static IUnitItem? VelocityMilesPerHour(this IUnitService service)
    {
        return service[VelocityUnit.Id, VelocityMilesPerHourUnitItem.Id];
    }

    public static IUnitItem? VelocityKilometerPerHour(this IUnitService service)
    {
        return service[VelocityUnit.Id, VelocityKilometersPerHourUnitItem.Id];
    }

    public static IUnitItem? VelocityMeterPerSecond(this IUnitService service)
    {
        return service[VelocityUnit.Id, VelocityMetersPerSecondUnitItem.Id];
    }
}
