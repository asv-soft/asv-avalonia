namespace Asv.Avalonia;

public static class AltitudeRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterAltitude(
        this UnitServiceRegistrations.Builder builder
    )
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
