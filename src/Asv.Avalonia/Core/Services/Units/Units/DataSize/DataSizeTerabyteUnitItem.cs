namespace Asv.Avalonia;

public sealed class DataSizeTerabyteUnitItem()
    : UnitItemBase(
        1.0
            / (
                DataSizeUnit.ScaleFactor
                * DataSizeUnit.ScaleFactor
                * DataSizeUnit.ScaleFactor
                * DataSizeUnit.ScaleFactor
            )
    )
{
    public const string Id = $"{DataSizeUnit.Id}.terabyte";

    public override string UnitItemId => Id;
    public override string Name => RS.Terabyte_UnitItem_Name;
    public override string Description => RS.Terabyte_DataSize_Description;
    public override string Symbol => RS.Unit_Terabyte_Abbreviation;
    public override bool IsInternationalSystemUnit => false;
}
