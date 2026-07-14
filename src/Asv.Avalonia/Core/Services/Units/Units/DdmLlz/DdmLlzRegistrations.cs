namespace Asv.Avalonia;

public static class DdmLlzRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterDdmLlz(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<DdmLlzUnit>(DdmLlzUnit.Id)
            .RegisterItem<DdmLlzNormalizedUnitItem>()
            .RegisterItem<DdmLlzMicroAmpRuUnitItem>()
            .RegisterItem<DdmLlzMicroAmpUnitItem>()
            .RegisterItem<DdmLlzPercentUnitItem>();
        return builder;
    }

    public static IUnitItem? DdmLlz(this IUnitService service) =>
        service[DdmLlzUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DdmLlzNormalized(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzNormalizedUnitItem.Id];

    public static IUnitItem? DdmLlzMicroAmpRu(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzMicroAmpRuUnitItem.Id];

    public static IUnitItem? DdmLlzMicroAmp(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzMicroAmpUnitItem.Id];

    public static IUnitItem? DdmLlzPercent(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzPercentUnitItem.Id];
}
