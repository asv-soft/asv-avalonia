namespace Asv.Avalonia;

public static class DistanceMixin
{
    public static UnitsBuilder RegisterDistance(this UnitsBuilder builder)
    {
        builder
            .AddUnit<DistanceUnit>(DistanceUnit.Id)
            .AddItem<DistanceMeterUnitItem>()
            .AddItem<DistanceNauticalMileUnitItem>();
        return builder;
    }

    public static IUnitItem? Distance(this IUnitService service) =>
        service[DistanceUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DistanceMeter(this IUnitService service) =>
        service[DistanceUnit.Id, DistanceMeterUnitItem.Id];

    public static IUnitItem? DistanceNauticalMiles(this IUnitService service) =>
        service[DistanceUnit.Id, DistanceNauticalMileUnitItem.Id];
}
