namespace Asv.Avalonia;

public static class TimeSpanMixin
{
    public static UnitsHostBuilderMixin.Builder RegisterTimeSpan(
        this UnitsHostBuilderMixin.Builder builder
    )
    {
        builder
            .AddUnit<TimeSpanUnit>(TimeSpanUnit.Id)
            .AddItem<TimeSpanMillisecondUnitItem>()
            .AddItem<TimeSpanMinuteSecondUnitItem>()
            .AddItem<TimeSpanHourMinuteUnitItem>()
            .AddItem<TimeSpanMinuteUnitItem>()
            .AddItem<TimeSpanHourMinuteSecondUnitItem>()
            .AddItem<TimeSpanHourUnitItem>()
            .AddItem<TimeSpanSecondUnitItem>();
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
