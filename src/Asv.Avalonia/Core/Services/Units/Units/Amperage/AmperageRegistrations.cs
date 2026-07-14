namespace Asv.Avalonia;

public static class AmperageRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterAmperage(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<AmperageUnit>(AmperageUnit.Id)
            .RegisterItem<AmperageAmpereUnitItem>()
            .RegisterItem<AmperageMilliAmpereUnitItem>();
        return builder;
    }

    public static IUnitItem? Amperage(this IUnitService service) =>
        service[AmperageUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? AmperageAmpere(this IUnitService service) =>
        service[AmperageUnit.Id, AmperageAmpereUnitItem.Id];

    public static IUnitItem? AmperageMilliAmpere(this IUnitService service) =>
        service[AmperageUnit.Id, AmperageMilliAmpereUnitItem.Id];
}
