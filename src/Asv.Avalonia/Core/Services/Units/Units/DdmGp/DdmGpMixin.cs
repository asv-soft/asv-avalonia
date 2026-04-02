namespace Asv.Avalonia;

public static class DdmGpMixin
{
    public static UnitsHostBuilderMixin.Builder RegisterDdmGp(
        this UnitsHostBuilderMixin.Builder builder
    )
    {
        builder
            .AddUnit<DdmGpUnit>(DdmGpUnit.Id)
            .AddItem<DdmGpInPartsUnitItem>()
            .AddItem<DdmGpMicroAmpRuUnitItem>()
            .AddItem<DdmGpMicroAmpUnitItem>()
            .AddItem<DdmGpPercentUnitItem>();
        return builder;
    }

    public static IUnitItem? DdmGp(this IUnitService service) =>
        service[DdmGpUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DdmGpInParts(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpInPartsUnitItem.Id];

    public static IUnitItem? DdmGpMicroAmpRu(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpMicroAmpRuUnitItem.Id];

    public static IUnitItem? DdmGpMicroAmp(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpMicroAmpUnitItem.Id];

    public static IUnitItem? DdmGpPercent(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpPercentUnitItem.Id];
}
