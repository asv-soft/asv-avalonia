namespace Asv.Avalonia;

public sealed class TimeSpanHourMinuteSecondUnitItem()
    : TimeSpanCompositeUnitItemBase(
        TimeSpanComponent.Hours,
        TimeSpanComponent.Minutes,
        TimeSpanComponent.Seconds
    )
{
    public const string Id = $"{TimeSpanUnit.Id}.hour-minute-second";

    public override string UnitItemId => Id;
    public override string Name => RS.HourMinuteSecond_UnitItem_Name;
    public override string Description => RS.HourMinuteSecond_TimeSpan_Description;
    public override string Symbol => RS.HourMinuteSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
