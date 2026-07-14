namespace Asv.Avalonia;

public static class ThrottleRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterThrottle(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<ThrottleUnit>(ThrottleUnit.Id)
            .RegisterItem<ThrottleNormalizedUnitItem>()
            .RegisterItem<ThrottlePercentUnitItem>();
        return builder;
    }

    public static IUnitItem? Throttle(this IUnitService service) =>
        service[ThrottleUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? ThrottleNormalized(this IUnitService service) =>
        service[ThrottleUnit.Id, ThrottleNormalizedUnitItem.Id];

    public static IUnitItem? ThrottlePercent(this IUnitService service) =>
        service[ThrottleUnit.Id, ThrottlePercentUnitItem.Id];
}
