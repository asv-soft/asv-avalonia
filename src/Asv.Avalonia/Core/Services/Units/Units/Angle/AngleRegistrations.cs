namespace Asv.Avalonia;

public static class AngleRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterAngle(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<AngleUnit>(AngleUnit.Id)
            .RegisterItem<AngleDegreeUnitItem>()
            .RegisterItem<AngleDmsUnitItem>()
            .RegisterItem<AngleMsUnitItem>();
        return builder;
    }

    public static IUnitItem? Angle(this IUnitService service) =>
        service[AngleUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? AngleDegree(this IUnitService service) =>
        service[AngleUnit.Id, AngleDegreeUnitItem.Id];

    public static IUnitItem? AngleDms(this IUnitService service) =>
        service[AngleUnit.Id, AngleDmsUnitItem.Id];

    public static IUnitItem? AngleMs(this IUnitService service) =>
        service[AngleUnit.Id, AngleMsUnitItem.Id];
}
