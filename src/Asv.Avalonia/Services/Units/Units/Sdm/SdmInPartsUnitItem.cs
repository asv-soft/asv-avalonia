namespace Asv.Avalonia;

public sealed class SdmInPartsUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{SdmUnit.Id}.parts";

    public override string UnitItemId => Id;
    public override string Name => RS.InParts_UnitItem_Name;
    public override string Description => RS.InParts_SDM_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => false;
}
