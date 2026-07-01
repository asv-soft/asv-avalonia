namespace Asv.Avalonia;

public static class AmModulationRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterAmModulation(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<AmModulationUnit>(AmModulationUnit.Id)
            .AddItem<AmModulationNormalizedUnitItem>()
            .AddItem<AmModulationPercentUnitItem>();
        return builder;
    }

    public static IUnitItem? AmModulation(this IUnitService service) =>
        service[AngleUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? AmModulationNormalized(this IUnitService service) =>
        service[AngleUnit.Id, AmModulationNormalizedUnitItem.Id];

    public static IUnitItem? AmModulationPercent(this IUnitService service) =>
        service[AngleUnit.Id, AmModulationPercentUnitItem.Id];
}
