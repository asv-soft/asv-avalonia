namespace Asv.Avalonia;

public static class ThrottleMixin
{
    public static UnitsHostBuilderMixin.Builder RegisterThrottle(
        this UnitsHostBuilderMixin.Builder builder
    )
    {
        builder
            .AddUnit<ThrottleUnit>(ThrottleUnit.Id)
            .AddItem<ThrottlePartUnitItem>()
            .AddItem<ThrottlePercentUnit>();
        return builder;
    }

    public static IUnitItem? Throttle(this IUnitService service) =>
        service[ThrottleUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? ThrottleInParts(this IUnitService service) =>
        service[ThrottleUnit.Id, ThrottlePartUnitItem.Id];

    public static IUnitItem? ThrottlePercent(this IUnitService service) =>
        service[ThrottleUnit.Id, ThrottlePercentUnit.Id];
}
