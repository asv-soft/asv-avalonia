namespace Asv.Avalonia;

public static class DistanceRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterDistance(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<DistanceUnit>(DistanceUnit.Id)
            .RegisterItem<DistanceMeterUnitItem>()
            .RegisterItem<DistanceNauticalMileUnitItem>();
        return builder;
    }

    public static IUnitItem? Distance(this IUnitService service) =>
        service[DistanceUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DistanceMeter(this IUnitService service) =>
        service[DistanceUnit.Id, DistanceMeterUnitItem.Id];

    public static IUnitItem? DistanceNauticalMiles(this IUnitService service) =>
        service[DistanceUnit.Id, DistanceNauticalMileUnitItem.Id];
}
