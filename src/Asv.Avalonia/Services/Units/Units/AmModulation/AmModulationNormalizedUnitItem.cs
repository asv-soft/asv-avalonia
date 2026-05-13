namespace Asv.Avalonia;

public sealed class AmModulationNormalizedUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{AmModulationUnit.Id}.normalized";

    public override string UnitItemId => Id;
    public override string Name => RS.Normalized_UnitItem_Name;
    public override string Description => RS.Normalized_AmplitudeModulation_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => true;
}
