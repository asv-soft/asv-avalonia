namespace Asv.Avalonia;

public static class SdmUnitRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterSdm(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<SdmUnit>(SdmUnit.Id)
            .AddItem<SdmPercentUnitItem>()
            .AddItem<SdmNormalizedUnitItem>();
        return builder;
    }

    public static IUnitItem? Sdm(this IUnitService service) =>
        service[SdmUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? SdmParts(this IUnitService service) =>
        service[SdmUnit.Id, SdmPercentUnitItem.Id];

    public static IUnitItem? SdmPercent(this IUnitService service) =>
        service[SdmUnit.Id, SdmNormalizedUnitItem.Id];
}
