namespace Asv.Avalonia;

public sealed class DdmLlzNormalizedUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{DdmLlzUnit.Id}.normalized";

    public override string UnitItemId => Id;
    public override string Name => RS.Normalized_UnitItem_Name;
    public override string Description => RS.Normalized_DdmLlz_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => true;
}
