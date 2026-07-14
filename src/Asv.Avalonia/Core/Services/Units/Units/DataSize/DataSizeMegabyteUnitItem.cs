namespace Asv.Avalonia;

public sealed class DataSizeMegabyteUnitItem()
    : UnitItemBase(1.0 / (DataSizeUnit.ScaleFactor * DataSizeUnit.ScaleFactor))
{
    public const string Id = $"{DataSizeUnit.Id}.megabyte";

    public override string UnitItemId => Id;
    public override string Name => RS.Megabyte_UnitItem_Name;
    public override string Description => RS.Megabyte_DataSize_Description;
    public override string Symbol => RS.Unit_Megabyte_Abbreviation;
    public override bool IsInternationalSystemUnit => false;
}
