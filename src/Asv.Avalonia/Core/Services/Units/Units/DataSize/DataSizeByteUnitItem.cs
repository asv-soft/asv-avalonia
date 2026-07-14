namespace Asv.Avalonia;

public sealed class DataSizeByteUnitItem() : UnitItemBase(1.0)
{
    public const string Id = $"{DataSizeUnit.Id}.byte";

    public override string UnitItemId => Id;
    public override string Name => RS.Byte_UnitItem_Name;
    public override string Description => RS.Byte_DataSize_Description;
    public override string Symbol => RS.Unit_Byte_Abbreviation;
    public override bool IsInternationalSystemUnit => true;
}
