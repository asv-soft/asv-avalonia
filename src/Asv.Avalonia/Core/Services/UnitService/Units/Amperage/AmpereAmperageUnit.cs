using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AmperageBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class AmpereAmperageUnit() : UnitItemBase(1)
{
    public const string Id = $"{AmperageBase.Id}.amp";

    public override string UnitItemId => Id;
    public override string Name => RS.Ampere_UnitItem_Name;
    public override string Description => RS.Amperage_AmpereUnit_Description;
    public override string Symbol => RS.Ampere_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
