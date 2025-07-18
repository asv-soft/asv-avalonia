using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(PhaseBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class RadianPhaseUnit() : UnitItemBase(180.0 / Math.PI)
{
    public const string Id = $"{PhaseBase.Id}.radian";

    public override string UnitItemId => Id;
    public override string Name => RS.Radian_UnitItem_Name;
    public override string Description => RS.Radian_Phase_Description;
    public override string Symbol => RS.Radian_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
