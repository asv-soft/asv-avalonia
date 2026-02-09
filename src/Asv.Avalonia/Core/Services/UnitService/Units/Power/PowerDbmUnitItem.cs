using System.Composition;

namespace Asv.Avalonia;

public sealed class PowerDbmUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{PowerUnit.Id}.dbm";

    public override string UnitItemId => Id;
    public override string Name => RS.Dbm_UnitItem_Name;
    public override string Description => RS.Dbm_Power_Description;
    public override string Symbol => RS.Dbm_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
