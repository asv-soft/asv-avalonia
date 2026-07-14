namespace Asv.Avalonia;

public static class TimeSpanRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterTimeSpan(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<TimeSpanUnit>(TimeSpanUnit.Id)
            .RegisterItem<TimeSpanMillisecondUnitItem>()
            .RegisterItem<TimeSpanMinuteSecondUnitItem>()
            .RegisterItem<TimeSpanHourMinuteUnitItem>()
            .RegisterItem<TimeSpanMinuteUnitItem>()
            .RegisterItem<TimeSpanHourMinuteSecondUnitItem>()
            .RegisterItem<TimeSpanHourUnitItem>()
            .RegisterItem<TimeSpanSecondUnitItem>();
        return builder;
    }

    public static IUnitItem? TimeSpan(this IUnitService service) =>
        service[TimeSpanUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? TimeSpanSecond(this IUnitService service) =>
        service[TimeSpanUnit.Id, TimeSpanSecondUnitItem.Id];

    public static IUnitItem? TimeSpanMinute(this IUnitService service) =>
        service[TimeSpanUnit.Id, TimeSpanMinuteUnitItem.Id];

    public static IUnitItem? TimeSpanHour(this IUnitService service) =>
        service[TimeSpanUnit.Id, TimeSpanHourUnitItem.Id];

    public static IUnitItem? TimeSpanHourMinuteSecond(this IUnitService service) =>
        service[TimeSpanUnit.Id, TimeSpanHourMinuteSecondUnitItem.Id];

    public static IUnitItem? TimeSpanMillisecond(this IUnitService service) =>
        service[TimeSpanUnit.Id, TimeSpanMillisecondUnitItem.Id];

    public static IUnitItem? TimeSpanMinuteSecond(this IUnitService service) =>
        service[TimeSpanUnit.Id, TimeSpanMinuteSecondUnitItem.Id];

    public static IUnitItem? TimeSpanHourMinute(this IUnitService service) =>
        service[TimeSpanUnit.Id, TimeSpanHourMinuteUnitItem.Id];
}
