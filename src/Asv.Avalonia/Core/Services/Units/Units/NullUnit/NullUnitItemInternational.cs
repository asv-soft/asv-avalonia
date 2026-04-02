namespace Asv.Avalonia;

public sealed class NullUnitItemInternational() : UnitItemBase(1)
{
    public override string UnitItemId => $"{NullUnitBase.Id}.item.international";
    public override string Name => "Null Inter";
    public override string Description => "Null unit item inter";
    public override string Symbol => "nI";
    public override bool IsInternationalSystemUnit => true;
}
