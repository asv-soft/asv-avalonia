namespace Asv.Avalonia;

public sealed class TimeSpanHourMinuteUnitItem()
    : TimeSpanCompositeUnitItemBase(TimeSpanComponent.Hours, TimeSpanComponent.Minutes)
{
    public const string Id = $"{TimeSpanUnit.Id}.hour-minute";

    public override string UnitItemId => Id;
    public override string Name => RS.HourMinute_UnitItem_Name;
    public override string Description => RS.HourMinute_TimeSpan_Description;
    public override string Symbol => RS.HourMinute_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
