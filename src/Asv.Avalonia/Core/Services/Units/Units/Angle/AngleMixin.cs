namespace Asv.Avalonia;

public static class AngleMixin
{
    public static UnitsHostBuilderMixin.Builder RegisterAngle(
        this UnitsHostBuilderMixin.Builder builder
    )
    {
        builder
            .AddUnit<AngleUnit>(AngleUnit.Id)
            .AddItem<AngleDegreeUnitItem>()
            .AddItem<AngleDmsUnitItem>()
            .AddItem<AngleMsUnitItem>();
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
