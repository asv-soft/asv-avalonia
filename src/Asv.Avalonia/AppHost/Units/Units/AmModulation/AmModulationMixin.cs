namespace Asv.Avalonia;

public static class AmModulationMixin
{
    public static  UnitsHostBuilderMixin.Builder RegisterAmModulation(this  UnitsHostBuilderMixin.Builder builder)
    {
        builder
            .AddUnit<AmModulationUnit>(AmModulationUnit.Id)
            .AddItem<AmModulationInPartsUnitItem>()
            .AddItem<AmModulationPercentUnitItem>();
        return builder;
    }

    public static IUnitItem? AmModulation(this IUnitService service) =>
        service[AngleUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? AmModulationInParts(this IUnitService service) =>
        service[AngleUnit.Id, AmModulationInPartsUnitItem.Id];

    public static IUnitItem? AmModulationPercent(this IUnitService service) =>
        service[AngleUnit.Id, AmModulationPercentUnitItem.Id];
}
