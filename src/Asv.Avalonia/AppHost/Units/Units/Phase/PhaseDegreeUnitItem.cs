namespace Asv.Avalonia;

public sealed class PhaseDegreeUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{PhaseUnit.Id}.degree";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Phase_Description;
    public override string Symbol => RS.Degree_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
