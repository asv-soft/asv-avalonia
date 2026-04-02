namespace Asv.Avalonia;

public sealed class TimeSpanHourUnitItem() : UnitItemBase(1 / 3600.0)
{
    public const string Id = $"{TimeSpanUnit.Id}.hour";

    public override string UnitItemId => Id;
    public override string Name => RS.Hour_UnitItem_Name;
    public override string Description => RS.Hour_TimeSpan_Description;
    public override string Symbol => RS.Hour_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
