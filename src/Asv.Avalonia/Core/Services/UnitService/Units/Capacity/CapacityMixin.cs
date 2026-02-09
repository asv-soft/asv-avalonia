namespace Asv.Avalonia;

public static class CapacityMixin
{
    public static UnitsBuilder RegisterCapacity(this UnitsBuilder builder)
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
