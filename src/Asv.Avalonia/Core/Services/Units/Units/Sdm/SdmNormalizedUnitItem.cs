namespace Asv.Avalonia;

public sealed class SdmNormalizedUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{SdmUnit.Id}.normalized";

    public override string UnitItemId => Id;
    public override string Name => RS.Normalized_UnitItem_Name;
    public override string Description => RS.Normalized_SDM_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => false;
}
