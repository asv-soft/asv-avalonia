namespace Asv.Avalonia;

public static class LatitudeRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterLatitude(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<LatitudeUnit>(LatitudeUnit.Id)
            .AddItem<LatitudeDmsUnitItem>()
            .AddItem<LatitudeDegreeUnitItem>();
        return builder;
    }

    public static IUnitItem? Latitude(this IUnitService service) =>
        service[LatitudeUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? LatitudeDms(this IUnitService service) =>
        service[LatitudeUnit.Id, LatitudeDmsUnitItem.Id];

    public static IUnitItem? LatitudeDegree(this IUnitService service) =>
        service[LatitudeUnit.Id, LatitudeDegreeUnitItem.Id];
}
