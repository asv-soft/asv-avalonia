namespace Asv.Avalonia;

public sealed class DdmGpInPartsUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{DdmGpUnit.Id}.in.parts";

    public override string UnitItemId => Id;
    public override string Name => RS.InParts_UnitItem_Name;
    public override string Description => RS.InParts_DdmGp_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => true;
}
