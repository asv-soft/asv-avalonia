namespace Asv.Avalonia;

public static class ThrottleMixin
{
    public static UnitsHostBuilderMixin.Builder RegisterThrottle(
        this UnitsHostBuilderMixin.Builder builder
    )
    {
        builder
            .AddUnit<ThrottleUnit>(ThrottleUnit.Id)
            .AddItem<ThrottleNormalizedUnitItem>()
            .AddItem<ThrottlePercentUnitItem>();
        return builder;
    }

    public static IUnitItem? Throttle(this IUnitService service) =>
        service[ThrottleUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? ThrottleNormalized(this IUnitService service) =>
        service[ThrottleUnit.Id, ThrottleNormalizedUnitItem.Id];

    public static IUnitItem? ThrottlePercent(this IUnitService service) =>
        service[ThrottleUnit.Id, ThrottlePercentUnitItem.Id];
}
