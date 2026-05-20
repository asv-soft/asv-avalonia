namespace Asv.Avalonia;

public sealed class TimeSpanMinuteSecondUnitItem()
    : TimeSpanCompositeUnitItemBase(TimeSpanComponent.Minutes, TimeSpanComponent.Seconds)
{
    public const string Id = $"{TimeSpanUnit.Id}.minute-second";

    public override string UnitItemId => Id;
    public override string Name => RS.MinuteSecond_UnitItem_Name;
    public override string Description => RS.MinuteSecond_TimeSpan_Description;
    public override string Symbol => RS.MinuteSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
