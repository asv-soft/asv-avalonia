namespace Asv.Avalonia;

public static class ProgressMixin
{
    public static UnitsHostBuilderMixin.Builder RegisterProgress(
        this UnitsHostBuilderMixin.Builder builder
    )
    {
        builder
            .AddUnit<ProgressUnit>(ProgressUnit.Id)
            .AddItem<ProgressPercentUnitItem>()
            .AddItem<ProgressInPartsUnitItem>();
        return builder;
    }

    public static IUnitItem? Progress(this IUnitService service) =>
        service[ProgressUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? ProgressParts(this IUnitService service) =>
        service[ProgressUnit.Id, ProgressPercentUnitItem.Id];

    public static IUnitItem? ProgressPercent(this IUnitService service) =>
        service[ProgressUnit.Id, ProgressInPartsUnitItem.Id];
}
