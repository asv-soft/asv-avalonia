namespace Asv.Avalonia;

public sealed class DataSizeGigabyteUnitItem()
    : UnitItemBase(
        1.0 / (DataSizeUnit.ScaleFactor * DataSizeUnit.ScaleFactor * DataSizeUnit.ScaleFactor)
    )
{
    public const string Id = $"{DataSizeUnit.Id}.gigabyte";

    public override string UnitItemId => Id;
    public override string Name => RS.Gigabyte_UnitItem_Name;
    public override string Description => RS.Gigabyte_DataSize_Description;
    public override string Symbol => RS.Unit_Gigabyte_Abbreviation;
    public override bool IsInternationalSystemUnit => false;
}
