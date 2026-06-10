namespace Asv.Avalonia;

public sealed class DdmGpNormalizedUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{DdmGpUnit.Id}.normalized";

    public override string UnitItemId => Id;
    public override string Name => RS.Normalized_UnitItem_Name;
    public override string Description => RS.Normalized_DdmGp_Description;
    public override string Symbol => "P";
    public override bool IsInternationalSystemUnit => true;
}
