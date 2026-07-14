namespace Asv.Avalonia;

public static class VelocityRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterVelocity(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<VelocityUnit>(VelocityUnit.Id)
            .RegisterItem<VelocityMetersPerSecondUnitItem>()
            .RegisterItem<VelocityKilometersPerHourUnitItem>()
            .RegisterItem<VelocityMilesPerHourUnitItem>();
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
