using System.Composition;

namespace Asv.Avalonia;

public sealed class AmperageAmpereUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{AmperageUnit.Id}.amp";

    public override string UnitItemId => Id;
    public override string Name => RS.Ampere_UnitItem_Name;
    public override string Description => RS.Amperage_AmpereUnit_Description;
    public override string Symbol => RS.Ampere_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
