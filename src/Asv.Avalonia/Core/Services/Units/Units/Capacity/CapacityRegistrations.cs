namespace Asv.Avalonia;

public static class CapacityRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterCapacity(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<CapacityUnit>(CapacityUnit.Id)
            .AddItem<CapacityMilliAmperePerHourUnitItem>();
        return builder;
    }

    public static IUnitItem? Capacity(this IUnitService service) =>
        service[CapacityUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? CapacityMilliAmperePerHour(this IUnitService service) =>
        service[CapacityUnit.Id, CapacityMilliAmperePerHourUnitItem.Id];
}
