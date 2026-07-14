namespace Asv.Avalonia;

public static class ProgressRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterProgress(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<ProgressUnit>(ProgressUnit.Id)
            .RegisterItem<ProgressPercentUnitItem>()
            .RegisterItem<ProgressNormalizedUnitItem>();
        return builder;
    }

    public static IUnitItem? Progress(this IUnitService service) =>
        service[ProgressUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? ProgressParts(this IUnitService service) =>
        service[ProgressUnit.Id, ProgressPercentUnitItem.Id];

    public static IUnitItem? ProgressPercent(this IUnitService service) =>
        service[ProgressUnit.Id, ProgressNormalizedUnitItem.Id];
}
