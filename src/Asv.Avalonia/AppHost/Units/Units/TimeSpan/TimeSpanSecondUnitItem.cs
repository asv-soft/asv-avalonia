namespace Asv.Avalonia;

public sealed class TimeSpanSecondUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{TimeSpanUnit.Id}.second";

    public override string UnitItemId => Id;
    public override string Name => RS.Second_UnitItem_Name;
    public override string Description => RS.Second_TimeSpan_Description;
    public override string Symbol => RS.Second_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
