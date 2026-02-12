namespace Asv.Avalonia;

public static class SdmUnitMixin
{
    public static  UnitsHostBuilderMixin.Builder RegisterSdm(this  UnitsHostBuilderMixin.Builder builder)
    {
        builder
            .AddUnit<SdmUnit>(SdmUnit.Id)
            .AddItem<SdmPercentUnitItem>()
            .AddItem<SdmInPartsUnitItem>();
        return builder;
    }

    public static IUnitItem? Sdm(this IUnitService service) =>
        service[SdmUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? SdmParts(this IUnitService service) =>
        service[SdmUnit.Id, SdmPercentUnitItem.Id];

    public static IUnitItem? SdmPercent(this IUnitService service) =>
        service[SdmUnit.Id, SdmInPartsUnitItem.Id];
}
