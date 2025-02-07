namespace Asv.Avalonia;

public class NullUnitItem() : UnitItemBase(2)
{
    public override string UnitItemId => $"{NullUnitBase.Id}.item";
    public override string Name => "Null";
    public override string Description => "Null unit item";
    public override string Symbol => "n";
    public override bool IsInternationalSystemUnit => false;
}
