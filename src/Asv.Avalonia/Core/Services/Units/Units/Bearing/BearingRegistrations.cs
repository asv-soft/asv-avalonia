namespace Asv.Avalonia;

public static class BearingRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterBearing(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<BearingUnit>(BearingUnit.Id)
            .AddItem<BearingDegreeUnitItem>()
            .AddItem<BearingDmUnitItem>();
        return builder;
    }

    public static IUnitItem? Bearing(this IUnitService service) =>
        service[BearingUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? BearingDegree(this IUnitService service) =>
        service[BearingUnit.Id, BearingDegreeUnitItem.Id];

    public static IUnitItem? BearingDm(this IUnitService service) =>
        service[BearingUnit.Id, BearingDmUnitItem.Id];
}
