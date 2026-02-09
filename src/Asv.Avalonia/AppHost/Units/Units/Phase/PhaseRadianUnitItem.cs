using System.Composition;

namespace Asv.Avalonia;

public sealed class PhaseRadianUnitItem() : UnitItemBase(180.0 / Math.PI)
{
    public const string Id = $"{PhaseUnit.Id}.radian";

    public override string UnitItemId => Id;
    public override string Name => RS.Radian_UnitItem_Name;
    public override string Description => RS.Radian_Phase_Description;
    public override string Symbol => RS.Radian_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
