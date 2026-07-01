namespace Asv.Avalonia;

public static class DdmGpRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterDdmGp(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<DdmGpUnit>(DdmGpUnit.Id)
            .AddItem<DdmGpNormalizedUnitItem>()
            .AddItem<DdmGpMicroAmpRuUnitItem>()
            .AddItem<DdmGpMicroAmpUnitItem>()
            .AddItem<DdmGpPercentUnitItem>();
        return builder;
    }

    public static IUnitItem? DdmGp(this IUnitService service) =>
        service[DdmGpUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DdmGpNormalized(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpNormalizedUnitItem.Id];

    public static IUnitItem? DdmGpMicroAmpRu(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpMicroAmpRuUnitItem.Id];

    public static IUnitItem? DdmGpMicroAmp(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpMicroAmpUnitItem.Id];

    public static IUnitItem? DdmGpPercent(this IUnitService service) =>
        service[DdmGpUnit.Id, DdmGpPercentUnitItem.Id];
}
