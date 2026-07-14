namespace Asv.Avalonia;

public sealed class DataSizeKilobyteUnitItem() : UnitItemBase(1.0 / DataSizeUnit.ScaleFactor)
{
    public const string Id = $"{DataSizeUnit.Id}.kilobyte";

    public override string UnitItemId => Id;
    public override string Name => RS.Kilobyte_UnitItem_Name;
    public override string Description => RS.Kilobyte_DataSize_Description;
    public override string Symbol => RS.Unit_Kilobyte_Abbreviation;
    public override bool IsInternationalSystemUnit => false;
}
