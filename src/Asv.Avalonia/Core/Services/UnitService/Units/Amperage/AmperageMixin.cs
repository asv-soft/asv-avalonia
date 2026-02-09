namespace Asv.Avalonia;

public static class AmperageMixin
{
    public static UnitsBuilder RegisterAmperage(this UnitsBuilder builder)
    {
        builder
            .AddUnit<AmperageUnit>(AmperageUnit.Id)
            .AddItem<AmperageAmpereUnitItem>()
            .AddItem<AmperageMilliAmpereUnitItem>();
        return builder;
    }

    public static IUnitItem? Amperage(this IUnitService service) =>
        service[AmperageUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? AmperageAmpere(this IUnitService service) =>
        service[AmperageUnit.Id, AmperageAmpereUnitItem.Id];

    public static IUnitItem? AmperageMilliAmpere(this IUnitService service) =>
        service[AmperageUnit.Id, AmperageMilliAmpereUnitItem.Id];
}
