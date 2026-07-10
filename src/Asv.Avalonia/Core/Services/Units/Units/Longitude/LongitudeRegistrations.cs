namespace Asv.Avalonia;

public static class LongitudeRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterLongitude(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<LongitudeUnit>(LongitudeUnit.Id)
            .AddItem<LongitudeDmsUnitItem>()
            .AddItem<LongitudeDegreeUnitItem>();
        return builder;
    }

    public static IUnitItem? Longitude(this IUnitService service) =>
        service[LongitudeUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? LongitudeDms(this IUnitService service) =>
        service[LongitudeUnit.Id, LongitudeDmsUnitItem.Id];

    public static IUnitItem? LongitudeDegree(this IUnitService service) =>
        service[LongitudeUnit.Id, LongitudeDegreeUnitItem.Id];
}
