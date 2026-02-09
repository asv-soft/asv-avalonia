namespace Asv.Avalonia;

public static class AltitudeMixin
{
    public static UnitsBuilder RegisterAltitude(this UnitsBuilder builder)
    {
        builder
            .AddUnit<AltitudeUnit>(AltitudeUnit.Id)
            .AddItem<AltitudeMeterUnitItem>()
            .AddItem<AltitudeFeetUnitItem>();
        return builder;
    }

    public static IUnitItem? Altitude(this IUnitService service) =>
        service[AltitudeUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? AltitudeMeter(this IUnitService service) =>
        service[AltitudeUnit.Id, AltitudeMeterUnitItem.Id];

    public static IUnitItem? AltitudeFeet(this IUnitService service) =>
        service[AltitudeUnit.Id, AltitudeFeetUnitItem.Id];
}
