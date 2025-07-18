using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(PhaseBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class DegreePhaseUnit() : UnitItemBase(1)
{
    public const string Id = $"{PhaseBase.Id}.degree";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Phase_Description;
    public override string Symbol => RS.Degree_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
