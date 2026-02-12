namespace Asv.Avalonia;

public static class DdmLlzMixin
{
    public static  UnitsHostBuilderMixin.Builder RegisterDdmLlz(this  UnitsHostBuilderMixin.Builder builder)
    {
        builder
            .AddUnit<DdmLlzUnit>(DdmLlzUnit.Id)
            .AddItem<DdmLlzInPartsUnitItem>()
            .AddItem<DdmLlzMicroAmpRuUnitItem>()
            .AddItem<DdmLlzMicroAmpUnitItem>()
            .AddItem<DdmLlzPercentUnitItem>();
        return builder;
    }

    public static IUnitItem? DdmLlz(this IUnitService service) =>
        service[DdmLlzUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DdmLlzInParts(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzInPartsUnitItem.Id];

    public static IUnitItem? DdmLlzMicroAmpRu(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzMicroAmpRuUnitItem.Id];

    public static IUnitItem? DdmLlzMicroAmp(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzMicroAmpUnitItem.Id];

    public static IUnitItem? DdmLlzPercent(this IUnitService service) =>
        service[DdmLlzUnit.Id, DdmLlzPercentUnitItem.Id];
}
